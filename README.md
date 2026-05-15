# SIDEImageFormat
SIDE (Simple Image Data Encoding) is a very basic compressed image file format.
## Format
The format is very simple. It is made up of three parts (Header, Palette, and Data) and is big-endian throughout the whole file.
### Header section
| Field name   | Field type | Notes                                       |
| ------------ | ---------- | ------------------------------------------- |
| Magic number | Bytes      | Always (in hex): 53 49 44 45 49 4D 47       |
| Image width  | Uint32     | The width of the image in pixels            |
| Image height | Uint32     | The height of the image in pixels           |
| Palette size | Uint32     | The amount of colors in the palette section |
### Palette section
| Field name | Field type                      | Notes                                                                |
| ---------- | ------------------------------- | -------------------------------------------------------------------- |
| Color      | ARGB color (8 bits per channel) | Repeated \[palette size\] times, sorted in descending order of usage |
### Data section
| Field name | Field type                      | Notes                                                                          |
| ---------- | ------------------------------- | ------------------------------------------------------------------------------ |
| RLE marker | Byte                            | A single zero byte before each index that is to be repeated                    |
| RLE count  | Varint32 (Protobuf compatible   | The amount of times the next index is to be repeated, present if RLE marker is |
| Index      | Varint32 (Protobuf compatible)  | Repeated \[width * height\] times, referring to an index in the palette        |
### Compression info
For each string of equivalent indices in the data section, the encoder checks if a run-length encoded version would be smaller than the raw version.
For instance, here's an example with the index 01 repeated 10 times.
```
01 01 01 01 01 01 01 01 01 01
```
This would get compressed down to the following (including the zero byte as a marker).
```
00 0A 01
```
In that case, RLE compression saved 7 bytes.
