import zmq
import base64
import json
from PIL import Image
import random;
from io import BytesIO
import numpy as np
import cv2
from keras.models import load_model


model = load_model("BehaviouralCloning_10.h5")

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")

# =================================================

def img_preprocess(img):
    img = img[125:200, :, :]
    img = cv2.cvtColor(img, cv2.COLOR_RGB2YUV)
    img = cv2.GaussianBlur(img,  (3, 3), 0)
    img = cv2.resize(img, (200, 66))
    img = img/255
    return img

while True:

    angle_adjustment = 0;
    message = socket.recv_string()

    # Decode the base64-encoded image
    image_data = base64.b64decode(message)

    image = Image.open(BytesIO(image_data))
    image = np.asarray(image)
    lane_image = np.copy(image)

    image = img_preprocess(image)
    image = np.array([image])

    pred = model.predict(image)


    print(pred, angle_adjustment);

    bytes_to_send = pred.tobytes()
    socket.send(bytes_to_send)