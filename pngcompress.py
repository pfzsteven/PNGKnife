# coding=utf-8

import os
import sys

from PIL import Image

ACTION_TYPE_CROP_AND_COMPRESS = 1
ACTION_TYPE_ONLY_COMPRESS = 2


def resize_image(image, output_path, size,quality):
    resized_image = image.resize(size)
    resized_image.save(output_path, optimize=True, quality=quality)
    pass


def compress_image(image, output_path, quality):
    image.save(output_path, optimize=True, quality=quality)
    pass


def do_scan(argv):
    dir_path = str(argv[2])
    if os.path.exists(dir_path):
        max_width = int(argv[3])
        max_height = int(argv[4])
        # print("Scanning... (%d,%d)" % (max_width, max_height))
        imageList = []
        for parent, dirnames, filenames in os.walk(dir_path):
            for fn in filenames:
                low_case = fn.lower()
                if low_case.endswith("png") or low_case.endswith("jpg") or low_case.endswith("tga"):
                    imageList.append(os.path.join(parent, fn))
                    pass
                pass
            pass
        for f in imageList:
            with Image.open(f) as img:
                width, height = img.size
                if width > max_width or height > max_height:
                    file_name = os.path.basename(f)
                    print("%s  -> Error Size: %dx%d" % (file_name, width, height))
                    pass
        pass
    else:
        pass
    pass


def do_crop_compress(action_type, image_type,dir_path,image_cut_size,compression_quality, min_width, min_height):
    if os.path.exists(dir_path):
        imageList = []
        for parent, dirnames, filenames in os.walk(dir_path):
            for fn in filenames:
                low_case = fn.lower()
                if image_type == "-a":
                    if low_case.endswith("png") or low_case.endswith("jpg") or low_case.endswith("tga"):
                        imageList.append(os.path.join(parent, fn))
                        pass
                    pass
                else:
                    if low_case.endswith(image_type):
                        imageList.append(os.path.join(parent, fn))
                        pass
                    pass
                pass
            pass
        pass
        for input_path in imageList:
            with Image.open(input_path) as img:
                width,height = img.size;
                if width > min_width and height > min_height:
                    if action_type == ACTION_TYPE_CROP_AND_COMPRESS:
                        print("%s %dx%d -> %dx%d Q:%d" % (input_path,width,height,image_cut_size[0], image_cut_size[1],compression_quality))
                        resize_image(img, input_path, image_cut_size,compression_quality)
                        pass
                    else:
                        print("%s --> Q:%d" % (input_path, compression_quality))
                        compress_image(img, input_path, compression_quality)
                        pass
            pass
        pass
    else:
        pass
    pass


if __name__ == '__main__':
    argv = sys.argv
    length = len(argv) - 1
    if length < 3:
        print("args error")
        sys.exit(0)
        pass
    if str(argv[1]) == "scan":
        do_scan(argv)
        pass
    else:
        condition_min_width = 0
        condition_min_height = 0
        action_type = ACTION_TYPE_ONLY_COMPRESS
        index = 0
        for x in argv:
            print("%s" % (argv[index]))
            index+=1
            if x == "-s":
                action_type = ACTION_TYPE_CROP_AND_COMPRESS
            pass
        pass
        image_type = str(argv[1]).lower()
        dir_path = argv[2]
        image_cut_size = (0, 0)
        compression_quality = 100

        if action_type == ACTION_TYPE_ONLY_COMPRESS:
            compression_quality = int(argv[4])
            condition_min_width = int(argv[6])
            condition_min_height = int(argv[7])
            pass
        else:
            image_cut_size = (int(argv[4]),int(argv[5]))
            compression_quality = int(argv[7])
            condition_min_width = int(argv[9])
            condition_min_height = int(argv[10])
            pass
        if compression_quality > 100:
            compression_quality = 100
            pass
        elif compression_quality < 1:
            compression_quality = 1
            pass
        pass
        do_crop_compress(action_type, image_type, dir_path,image_cut_size,compression_quality,condition_min_width,condition_min_height)
        pass

