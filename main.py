import zmq
import base64
import json
from PIL import Image
import random;
from io import BytesIO
import numpy as np
import cv2
from keras.models import load_model


model = load_model("model-3.h5")

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")


# TODO:
# 1) Try blending between the prediction of the model and the turn angle from image
# 2) Try adjusting the image line detection criteria in some way so that it always detects ONLY the lanes
# 3) Fix the UI and functionality
# 4) Add some traffic signs
# 5) Adjust the traffic sign detection code
# 6) Maybe add some laser for the purpouse of simulating LiDAR sensors, so that if there's an object nearby, it tries to avoid it
# 7) As for the signs, MAYBE you can somehow add functionality for, let's say, only right turn sign, so that we turn to the right after that sign
# 8) A bit far-fetched, but maybe some path tracking code might be great.





# =================== LANE DETECTION =====================

def make_coordinates(image, line_parameter):
    slope, intercept = line_parameter;
    y1 = image.shape[0]
    y2 = int(y1*(3/5))
    if slope != 0:  # Add this check to avoid division by zero
        x1 = int((y1 - intercept) / slope)
        x2 = int((y2 - intercept) / slope)
        return np.array([x1, y1, x2, y2]);
    else:
        # Handle the case when the slope is zero (vertical line)
        x1 = x2 = int(intercept)
        return np.array([x1, y1, x2, y2]);

def average_slope_intercept(image, lines):
    left_fit = []
    right_fit = []

    for line in lines:
        x1, y1, x2, y2 = line.reshape(4);
        parameters = np.polyfit((x1, x2), (y1, y2), 1);
        slope = parameters[0]
        intercept = parameters[1]
        if (slope < 0):
            left_fit.append((slope, intercept))
        else:
            right_fit.append((slope, intercept))
    if (len(left_fit) > 0 and len(right_fit) > 0):
        left_fit_average = np.average(left_fit, axis=0);
        right_fit_average = np.average(right_fit, axis=0)
        left_line = make_coordinates(image, left_fit_average);
        right_line = make_coordinates(image, right_fit_average);
        return np.array([left_line, right_line]);
    else:
        return np.array([np.array([0, 0, 1, 1]), np.array([256, 256, 255, 255])]);




def canny(image):
    gray = cv2.cvtColor(image, cv2.COLOR_RGB2GRAY)
    blur = cv2.GaussianBlur(gray, (5, 5), 0)
    canny = cv2.Canny(blur, 50, 150)
    return canny;

def display_lines(image, lines):
    line_image = np.zeros_like(image)
    if lines is not None:
        for x1, y1, x2, y2 in lines:
            cv2.line(line_image, (x1, y1), (x2, y2), (255, 0, 0), 2)
    return line_image

def region_of_interest(image):
    height = image.shape[0]
    polygons = np.array([[(0, 162), (256, 162), (125, 124)]])
    mask = np.zeros_like(image)
    cv2.fillPoly(mask, [polygons], 255)
    masked_image = cv2.bitwise_and(image, mask)
    return masked_image;

def calculate_angle_adjustment(lines):
    center = (140, 205)

    # Calculate Lane Midpoints
    lane_midpoints = []
    for line in lines:
        x1, y1, x2, y2 = line
        midpoint = [(x1 + x2) / 2, (y1 + y2) / 2]
        lane_midpoints.append(midpoint)

    # Calculate Lane Center
    lane_center = np.mean(lane_midpoints, axis=0)

    # Calculate Angle Adjustment
    angle_adjustment = np.arctan2(lane_center[1] - center[1], lane_center[0] - center[0])

    return angle_adjustment

# =================================================

def img_preprocess(img):
    # img = mpimg.imread(img);
    img = img[125:200, :, :]
    img = cv2.cvtColor(img, cv2.COLOR_RGB2YUV)
    img = cv2.GaussianBlur(img,  (3, 3), 0)
    img = cv2.resize(img, (200, 66))
    # img.astype(np.float32) / 255.0
    return img

while True:

    angle_adjustment = 0;
    # Wait for next request from client
    message = socket.recv_string()
    # data = json.loads(message)

    # Decode the base64-encoded image
    image_data = base64.b64decode(message)

    # Open the image using Pillow (PIL)
    image = Image.open(BytesIO(image_data))
    image = np.asarray(image)

    lane_image = np.copy(image)

    # Apply your preprocessing function
    image = img_preprocess(image)

    image = np.array([image])

    # ========== CODE FOR CALCULATING LANES =================
    canny_image = canny(lane_image)
    cropped_image = region_of_interest(canny_image)
    lines = cv2.HoughLinesP(cropped_image, 1, np.pi/180, 40, np.array([]), minLineLength=5, maxLineGap=5)
    if lines is not None:
        averaged_lines = average_slope_intercept(lane_image, lines)
        # line_image = display_lines(lane_image, averaged_lines)
        # combo_image = cv2.addWeighted(lane_image, 0.8, line_image, 1, 1)
        angle_adjustment = calculate_angle_adjustment(averaged_lines)

    pred = model.predict(image)

    # detect lanes of the image
    

    # Save the processed image
    # cv2.imwrite("grr_.jpg", (image * 255).astype(np.uint8))


    print(pred, angle_adjustment);

    # Convert the prediction to bytes and send it back
    if (angle_adjustment != 0):
        # bytes_to_send = [[angle_adjustment]].tobytes()
        bytes_to_send = np.array([angle_adjustment]).tobytes();
        socket.send(bytes_to_send)
    else:
        bytes_to_send = pred.tobytes()
        socket.send(bytes_to_send)
