import numpy as np # linear algebra
import pandas as pd # data processing, CSV file I/O (e.g. pd.read_csv)
import cv2
import time
from timeit import default_timer as timer
import matplotlib.pyplot as plt
from keras.models import load_model

model = load_model('sign_classification.h5')

labels = pd.read_csv('./input/signnames.csv')

img = cv2.imread('test_image.png')

# c_ts = img[y_min:y_min+int(box_height), x_min:x_min+int(box_width), :]


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
probability_minimum = 0.2

# non-maximum suppression treshold? (i really need to look more into it)
treshold = 0.2

colors = np.random.randint(0, 255, size=(len(labels), 3), dtype='uint8')

print(type(colors))  # <class 'numpy.ndarray'>
print(colors.shape)  # (43, 3)
print(colors[0])  # [25  65 200]

print(img.shape)



t = 0
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

# Going through all output layers after feed forward pass
for result in output_from_network:
    # Going through all detections from current output layer
    for detected_objects in result:
        # Getting 80 classes' probabilities for current detected object
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


#color isn't something to focus on, but should focus more on the shape, edges, etc
def grayscale(img):
  img = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
  return img

def equalize(img):
  img = cv2.equalizeHist(img)
  return img

def preprocessing(img):
  img = grayscale(img)
  img = equalize(img)
  # normalization makes px intesity between 0 and 1
  img = img/255
  return img


# Checking if there is any detected object been left
if len(results) > 0:
    # Going through indexes of results
    for i in results.flatten():
        # Bounding box coordinates, its width and height
        x_min, y_min = bounding_boxes[i][0], bounding_boxes[i][1]
        box_width, box_height = bounding_boxes[i][2], bounding_boxes[i][3]


        # Cut fragment with Traffic Sign
        c_ts = img[y_min:y_min+int(box_height), x_min:x_min+int(box_width), :]
        # print(c_ts.shape)

        if c_ts.shape[:1] == (0,) or c_ts.shape[1:2] == (0,):
            pass
        else:
            # # Convert the image to grayscale
            # img_gray = cv2.cvtColor(c_ts, cv2.COLOR_BGR2GRAY)
            # # Resize the image to match the model's input shape
            # img_resized = cv2.resize(img_gray, (32, 32))
            # # Normalize the pixel values to the range [0, 1]
            # img_normalized = img_resized / 255.0
            # # Add a channel dimension to match the model's input shape
            # img_input = np.expand_dims(img_normalized, axis=-1)
            # img_input = np.expand_dims(img_input, axis=0)  # Add batch dimension

            img_input = cv2.resize(c_ts, (32, 32))
            img_input = preprocessing(img_input)
            # plt.imshow(img_input, cmap = plt.get_cmap('gray'))
            img_input = img_input.reshape(1, 32, 32, 1)

            cv2.imwrite('result-sign.jpg', (img_input.squeeze() * 255).astype(np.uint8))

            # Predict using the model
            scores = model.predict(img_input)
            prediction = np.argmax(scores)
            class_name = labels['SignName'][prediction]
            print("PREDICTED: ", class_name)

            # Draw bounding box on the original image
            cv2.rectangle(img, (x_min, y_min), (x_min + box_width, y_min + box_height), (0, 255, 0), 2)
            cv2.putText(img, class_name, (x_min, y_min - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.9, (0, 255, 0), 2)

            # # Colour for current bounding box
            # colour_box_current = colors[class_numbers[i]].tolist()
            #
            # # Green BGR
            # colour_box_current = [0, 255, 61]
            #
            # # Yellow BGR
            # # colour_box_current = [0, 255, 255]
            #
            # # Drawing bounding box on the original current frame
            # cv2.rectangle(img, (x_min, y_min),
            #               (x_min + box_width, y_min + box_height),
            #               colour_box_current, 6)


# Saving image
cv2.imwrite('result.png', img)
cv2.imshow("Image", img)
cv2.waitKey(0)
cv2.destroyAllWindows()
