# PNGKnife

重要声明:该脚本会在原来文件基础上进行覆盖处理，所以要裁剪时，最好是先**备份**原文件夹！！！

使用python脚本批量裁剪png图片尺寸大小。使用说明:

```python
python <pngcompress.py文件绝对路径> <文件格式,如:png> <文件夹路径> <输出宽度> <输出高度>
```

## 示例(mac平台)

```python
python /Users/admin/pngcompress.py png /Users/admin/压缩图文件夹 200 200
```

# 无法执行解决方案

## 解决zsh "pip command not found"

> chsh -s /bin/bash

## 依赖库找不到

> 目前均在python 3.7环境下开发，如果还是处于2.7的，需要升级下pip。这边不做介绍。

先执行命令 `pip3 install image`
再执行命令 `pip3 install PIL`
