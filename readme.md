#BloomFilter for Monogame and XNA

A Bloom filter usable for Monogame and XNA applications. 

Included is a sample solution, which shows the basic setup and how the integration can look like.

For the effect itself you only need the BloomFilter.cs and Shaders/BloomFilter/Bloom.fx files. 
This is a Windows desktop build, with DirectX. 
I presume it works for OpenGL, too, but you would have to change the shader model from 4_0 to a lower version.

The default rendertargets are of Format.Color, but if you do HDR or fp16 in general you should switch them up to fp16, too.

![Alt text](http://i.imgur.com/jV6DWB5.png "Sample Application")
 
    High-Quality Bloom filter for high-performance applications

    Based largely on the implementations in Unreal Engine 4 and Call of Duty AW
    For more information look for

    "Next Generation Post Processing in Call of Duty Advanced Warfare" by Jorge Jimenez

    http://www.iryoku.com/downloads/Next-Generation-Post-Processing-in-Call-of-Duty-Advanced-Warfare-v18.pptx

    
    The idea is to have several rendertargets or one rendertarget with several mip maps
    so each mip has half resolution (1/2 width and 1/2 height) of the previous one.

    32, 16, 8, 4, 2

    In the first step we extract the bright spots from the original image. If not specified otherwise thsi happens in full resolution.
    We can do that based on the average RGB value or Luminance and check whether this value is higher than our Threshold.

        BloomUseLuminance = true / false (default is true)

        BloomThreshold = 0.8f;

    Then we downscale this extraction layer to the next mip map.
    While doing that we sample several pixels around the origin.

    We continue to downsample a few more times, defined in

        BloomDownsamplePasses = 5 ( default is 5)

    Afterwards we upsample again, but blur in this step, too.
    The final output should be a blur with a very large kernel and smooth gradient.

    The output in the draw is only the blurred extracted texture. 
    It can be drawn on top of / merged with the original image with an additive operation for example.

    If you use ToneMapping you should apply Bloom before that step.


