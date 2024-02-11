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

def img_preprocess(img):
    # img = mpimg.imread(img);
    img = img[125:200, :, :]
    img = cv2.cvtColor(img, cv2.COLOR_RGB2YUV)
    img = cv2.GaussianBlur(img,  (3, 3), 0)
    img = cv2.resize(img, (200, 66))
    # img.astype(np.float32) / 255.0
    return img

while True:
    # Wait for next request from client
    message = socket.recv_string()
    # data = json.loads(message)

    # Decode the base64-encoded image
    image_data = base64.b64decode(message)

    # Open the image using Pillow (PIL)
    image = Image.open(BytesIO(image_data))
    image = np.asarray(image)

    # Apply your preprocessing function
    image = img_preprocess(image)

    image = np.array([image])

    pred = model.predict(image)

    # Save the processed image
    # cv2.imwrite("grr_.jpg", (image * 255).astype(np.uint8))


    print(pred);

    # Convert the prediction to bytes and send it back
    bytes_to_send = pred.tobytes()
    socket.send(bytes_to_send)
