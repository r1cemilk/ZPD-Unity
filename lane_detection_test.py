import cv2
import numpy as np
import matplotlib.pyplot as plt


def make_coordinates(image, line_parameter):
    slope, intercept = line_parameter;
    y1 = image.shape[0]
    y2 = int(y1*(3/5))
    x1 = int((y1 - intercept)/slope)
    x2 = int((y2 - intercept)/slope)
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
    left_fit_average = np.average(left_fit, axis=0);
    right_fit_average = np.average(right_fit, axis=0)
    left_line = make_coordinates(image, left_fit_average);
    right_line = make_coordinates(image, right_fit_average);
    return np.array([left_line, right_line]);





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

image = cv2.imread('result.png')
lane_image = np.copy(image)
canny_image = canny(lane_image)
cropped_image = region_of_interest(canny_image)
lines = cv2.HoughLinesP(cropped_image, 2, np.pi/180, 10, np.array([]), minLineLength=1, maxLineGap=5)
averaged_lines = average_slope_intercept(lane_image, lines)
line_image = display_lines(lane_image, averaged_lines)
combo_image = cv2.addWeighted(lane_image, 0.8, line_image, 1, 1)


cv2.imshow("result", combo_image)
cv2.waitKey(0)

# def make_coordinates(image, line_paramaters):
# 	slope, intercept = line_paramaters
# 	y1 = image.shape[0]
# 	y2 = int(y1*(3/5))
	
# 	x1 = ((y1-intercept)/slope)# izsaka no y=ax + b
# 	x2 = ((y2-intercept)/slope)
# 	return np.array([x1, y1, x2, y2])

# def average_slope_intercept(image, lines):
# 	left_fit=[] #average lines on the left
# 	right_fit=[] # exact opposite
# 	for line in lines:
# 		x1, y1, x2, y2 = line.reshape(4)
# 		parameters=np.polyfit((x1, x2), (y1, y2), 1) # dabūn y=ax + b (dabūn a)
# 		slope=parameters[0]
# 		intercept=parameters[1]
# 		if slope < 0:
# 			left_fit.append((slope, intercept)) 
# 		else:
# 			right_fit.append((slope, intercept))
# 	left_fit_average=np.average(left_fit, axis=0)
# 	right_fit_average=np.average(right_fit, axis=0)
# 	left_line=make_coordinates(image, left_fit_average)
# 	right_line=make_coordinates(image, right_fit_average)
# 	return np.array([left_line, right_line])

 
# def canny(img):
# 	gray = cv2.cvtColor(img, cv2.COLOR_RGB2GRAY) # convert image to grayscale
# 	blur = cv2.GaussianBlur(gray, (5, 5), 0) # adding blur to image(to reduce noice, for some reason)(using 5x5 grid for blurring)
# 	canny = cv2.Canny(blur, 50, 150) # detecting rough edges
# 	return canny

# def region_of_interest(img):
# 	height=img.shape[0]
# 	polygons=np.array([[(0, height), (256, height), (100, 169)]])
# 	mask=np.zeros_like(img)
# 	cv2.fillPoly(mask, polygons, 255)
# 	masked_image=cv2.bitwise_and(img, mask) #
# 	return masked_image

# def display_lines(image, lines):
# 	line_image=np.zeros_like(image)
# 	if lines is not None:
# 		for x1, y1, x2, y2 in lines:
# 			cv2.line(line_image, (int(x1), int(y1)), (int(x2), int(y2)), (255, 0, 0), 10)
# 	return line_image


# image = cv2.imread('result.png') # returns a matrix, each number corrseponding to a pixel intesity

# lane_image=np.copy(image) # duplicating image
# canny_image = canny(lane_image)
# cropped_image = region_of_interest(canny_image)
# lines = cv2.HoughLinesP(cropped_image, 2, np.pi/180, 100, np.array([]), minLineLength=40, maxLineGap=5) # Ok, so basically, we have multiple, curver (or not) lines, and we define a grid that counts the number of points in each grid tab, and the one with the most points in it is the one we're basing our singe line from
# # houghLinesP(the image with lines, the number of px to create the grid at, angle in radiants, threshold)
# #whatever, just look at section 1 video 10 or so. Cause this line is very fucking important

# average_lines = average_slope_intercept(lane_image, lines)

# line_image=display_lines(lane_image, average_lines)
# merged_image=cv2.addWeighted(lane_image, 0.8, line_image, 1, 1)
# cv2.imshow('result', merged_image)
# cv2.waitKey(0)
# cv2.destroyAllWindows()
