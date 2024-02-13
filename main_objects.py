import numpy as np # linear algebra
import pandas as pd # data processing, CSV file I/O (e.g. pd.read_csv)
import cv2
from PIL import Image
from io import BytesIO
import base64
import zmq
import time
from timeit import default_timer as timer
import matplotlib.pyplot as plt
from keras.models import load_model

model = load_model('sign_classification.h5')
labels = pd.read_csv('./input/signnames.csv')

# === CODE FOR SOCKET

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:4242")

# ===================

# input image width and height
whT = 256;
threshold = 0.2;
# confidence treshold
confTreshold = 0.5;
# Non-maximum suppression treshold (whatever that is)
nmsTreshold = 0.3;

path_to_weights = './input/yolov3_ts_train_final.weights'
path_to_cfg = './input/yolov3_ts_test.cfg'

network = cv2.dnn.readNetFromDarknet(path_to_cfg, path_to_weights)
network.setPreferableBackend(cv2.dnn.DNN_BACKEND_OPENCV)
network.setPreferableTarget(cv2.dnn.DNN_TARGET_OPENCL_FP16)

#layer names
layers_all = network.getLayerNames()

# getting only detection layers
layers_names_output = []
for i in network.getUnconnectedOutLayers():
    layers_names_output.append(layers_all[i - 1])

# minimum propability
probability_minimum = 0.9

colors = np.random.randint(0, 255, size=(len(labels), 3), dtype='uint8')






# ===== PREPOCESSING
def grayscale(img):
  img = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
  return img

def equalize(img):
  img = cv2.equalizeHist(img)
  return img

def preprocessing(img):
  img = grayscale(img)
  img = equalize(img)
  img = img/255
  return img


# ==================

while True:

  # getting the image
  message = socket.recv_string()
  image_data = base64.b64decode(message)
  img = Image.open(BytesIO(image_data))
  img = np.asarray(img)
  img = cv2.cvtColor(img, cv2.COLOR_RGB2BGR)
  # img = np.array([img])
  # img = cv2.resize(img, (416, 416))

  t = 0
  # h, w = img.shape[:2]

  h, w = img.shape[:2]


  # need to convert image to blob for yolov3
  blob = cv2.dnn.blobFromImage(img, 1/255.0, (416, 416), swapRB=True, crop=False)

  # Forward pass with blob through output layers
  network.setInput(blob)
  start = time.time()
  output_from_network = network.forward(layers_names_output)
  end = time.time()

  # Time
  t += end - start
  print('Total amount of time {:.5f} seconds'.format(t))


  bounding_boxes = []
  confidences = []
  class_numbers = []

  bytes_to_send = b'12421';



  # Going through all output layers after feed forward pass
  for result in output_from_network:
      # Going through all detections from current output layer
      for detected_objects in result:
          # TODO: Figure out what this does
          scores = detected_objects[5:]
          # Getting index of the class with the maximum value of probability
          class_current = np.argmax(scores)
          # Getting value of probability for defined class
          confidence_current = scores[class_current]

          # Eliminating weak predictions by minimum probability
          if confidence_current > probability_minimum:
              # Scaling bounding box coordinates to the initial frame size
              box_current = detected_objects[0:4] * np.array([w, h, w, h])

              # Getting top left corner coordinates
              x_center, y_center, box_width, box_height = box_current
              x_min = int(x_center - (box_width / 2))
              y_min = int(y_center - (box_height / 2))

              # Adding results into prepared lists
              bounding_boxes.append([x_min, y_min, int(box_width), int(box_height)])
              confidences.append(float(confidence_current))
              class_numbers.append(class_current)


  # Implementing non-maximum suppression of given bounding boxes
  results = cv2.dnn.NMSBoxes(bounding_boxes, confidences, probability_minimum, threshold)

  if len(results) > 0:
      for i in results.flatten():
          # Bounding box coordinates, its width and height
          x_min, y_min = bounding_boxes[i][0], bounding_boxes[i][1]
          box_width, box_height = bounding_boxes[i][2], bounding_boxes[i][3]


          # Cut fragment with Traffic Sign
          c_ts = img[y_min:y_min+int(box_height), x_min:x_min+int(box_width), :]

          if c_ts.shape[:1] == (0,) or c_ts.shape[1:2] == (0,):
              pass
          else:
              img_input = cv2.resize(c_ts, (32, 32))
              img_input = preprocessing(img_input)
              img_input = img_input.reshape(1, 32, 32, 1)

              cv2.imwrite('result-sign.jpg', (img_input.squeeze() * 255).astype(np.uint8))

              # Predict using the model
              scores = model.predict(img_input)
              prediction = np.argmax(scores)
              class_name = labels['SignName'][prediction]
              print("PREDICTED: ", class_name)

              # return image to unity
              bytes_to_send = prediction.tobytes()


              # # Draw bounding box on the original image
              cv2.rectangle(img, (x_min, y_min), (x_min + box_width, y_min + box_height), (0, 255, 0), 2)
              cv2.putText(img, class_name, (x_min, y_min - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.9, (0, 255, 0), 2)
              # pred = 0.2743;
  cv2.imwrite('result.png', img)
  socket.send(bytes_to_send)