import threading
import time
import numpy as np
import cv2
import pyrealsense2 as rs
from ultralytics import YOLO
from flask import Flask, jsonify

# ========== Global Shared Data ==========
latest_coords = {
    "x": None,
    "y": None,
    "z": None,
    "distance": None
}

# ========== Flask App ==========
app = Flask(__name__)

@app.route('/coords')
def get_coords():
    return jsonify(latest_coords)

# ========== YOLO + RealSense Detection ==========
def detect_loop():
    model = YOLO('best.pt')

    pipeline = rs.pipeline()
    config = rs.config()
    config.enable_stream(rs.stream.depth, 640, 480, rs.format.z16, 30)
    config.enable_stream(rs.stream.color, 640, 480, rs.format.bgr8, 30)
    pipeline.start(config)

    align_to = rs.stream.color
    align = rs.align(align_to)
    depth_scale = pipeline.get_active_profile().get_device().first_depth_sensor().get_depth_scale()
    print(f"[INFO] Depth Scale: {depth_scale}")

    try:
        while True:
            frames = pipeline.wait_for_frames()
            aligned_frames = align.process(frames)
            depth_frame = aligned_frames.get_depth_frame()
            color_frame = aligned_frames.get_color_frame()

            if not depth_frame or not color_frame:
                continue

            depth_image = np.asanyarray(depth_frame.get_data())
            color_image = np.asanyarray(color_frame.get_data())

            results = model(color_image)[0]
            for det in results.boxes:
                x1, y1, x2, y2 = det.xyxy[0]
                conf = det.conf[0]
                if conf < 0.6:
                    continue

                center_x = int((x1 + x2) / 2)
                center_y = int((y1 + y2) / 2)
                depth_value = depth_image[center_y, center_x] * depth_scale

                if depth_value == 0:
                    continue  # skip invalid depth

                intrinsics = depth_frame.profile.as_video_stream_profile().intrinsics
                x, y, z = rs.rs2_deproject_pixel_to_point(intrinsics, [center_x, center_y], depth_value)
                distance = np.linalg.norm([x, y, z])

                # Update shared variable
                latest_coords["x"] = float(x)
                latest_coords["y"] = float(y)
                latest_coords["z"] = float(z)
                latest_coords["distance"] = float(distance)

                break  # only one object at a time for simplicity
    finally:
        pipeline.stop()

# ========== Run Everything ==========
if __name__ == '__main__':
    # Start detection loop in background thread
    t = threading.Thread(target=detect_loop, daemon=True)
    t.start()

    # Start Flask server
    app.run(host='0.0.0.0', port=5000, debug=False)
