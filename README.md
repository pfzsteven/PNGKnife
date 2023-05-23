# Tips

1. It supports to resize and compress all image files in the folder. 
2. It supports for integration into the unity project.
3. Advice backup your images directory before executing script.

## Terminal Usage

### Step 1. Download Script.

Download the `pngcompress.py` file in your location '/Users/Admin/pngcompress.py'(Max OS) or 'C://Users//Admin//pngcompress.py'(Windows OS)

### Step 2. Choose one of the features to execute in your Terminal.

- Scan and print out the image file path that meets the size conditions

```shell
python3 pngcompress scan <image folder> 
                         <large than width> <large than height>
```

- Resize or Compress the image file, and replace to own path.

```shell
python3 pngcompress <media type> 
                    <images folder> 
                    -s <output width> <output height> 
                    -q <quality> 
                    -w <larger than width> <larger than height>
```
**Args**

- media type :There are 2 Options: (1)Exactly a type: `png` or `jpg` or `tga`; (2)All types: `-a`
- images folder:Input a directory path. E.g: `/Users/Icons`
- [Option]-s(Short name of size): Resize the image width and height. Value is range `[1,Int.max]`.
- [Option]-q(Short name of quality): Compress the image quality. Value is range `[1,100]`
- [Option]-w(Short name of where): Filter the image width and height to be executed. If not declared, all images will be executed.

#### Features

**1.Scan Only**

```shell
python3 pngcompress scan 1000 1000
```


**2.Resize and Compress Quality**

```shell
python3 pngcompress png /Users/Icons -s 128 128 -q 95 -w 256 256

or you want the all images to do
python3 pngcompress -a /Users/Icons -s 128 128 -q 95

or you add a size filter.
python3 pngcompress -a /Users/Icons -s 128 128 -q 95 -w 256 256
```

**3.Resize Only**

```shell
python3 pngcompress png /Users/Icons -s 128 128

or you want the all images to do
python3 pngcompress -a /Users/Icons -s 128 128

or you add a size filter.
python3 pngcompress -a /Users/Icons -s 128 128 -w 256 256
```

**4.Compress Only**

```shell
python3 pngcompress png /Users/Icons -q 80

or you want the all images to do
python3 pngcompress -a /Users/Icons -q 80

or you add a size filter.
python3 pngcompress -a /Users/Icons -q 80 -w 256 256
```

## Unity EidtorWindow Usage

1. Create a directory named like `Python` under the `Assets` folder (`/Assets/Python/`).
2. Copy the `pngcompress.py` into the `Python` folder.
3. Copy the C# script into the `Editor` folder.
4. Opening the EditorWindow in your UnityEditor.

