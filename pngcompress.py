# coding=utf-8

import os
import sys

from PIL import Image

if __name__ == '__main__':
    print("使用说明: \npython pngcompress <文件格式,如:png> <文件夹路径> <输出宽度> <输出高度>\n")
    if (len(sys.argv)) < 4:
        print("参数错误，参考使用说明")
        sys.exit(0)
        pass
    image_type = sys.argv[1]
    dir_path = sys.argv[2]
    image_cut_size = (int(sys.argv[3]), int(sys.argv[4]))
    if os.path.exists(dir_path):
        imageList = []
        for parent, dirnames, filenames in os.walk(dir_path):
            for fn in filenames:
                if fn.endswith(image_type):
                    imageList.append(os.path.join(parent, fn))
                    pass
                pass
            pass
        print("图片总数:%d" % len(imageList))
        for f in imageList:
            img = Image.open(f)
            f_name = img.filename.split('/')[-1]
            dir_name = img.filename.split('/')[-2]
            out = img.resize(image_cut_size)
            out.save(img.filename)
            pass
        print("处理完成~")
