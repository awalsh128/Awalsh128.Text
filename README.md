### Awalsh128 Text Processing Library

- Provides Unix-to-Unix encoding support using the .NET Stream abstraction.
- Library is provided under the terms of the LGPL version 2.

To decode any stream:
```csharp
using (Stream encodedStream = /* Any readable stream. */)
using (Stream decodedStream = /* Any writeable stream. */)
using (var decodeStream = new UUDecodeStream(encodedStream))
{ 
    decodeStream.CopyTo(decodedStream);
    // Decoded contents are now in decodedStream.
}
```

To encode any stream:
```csharp
bool unixLineEnding = /* True if encoding with Unix line endings, otherwise false.
using (Stream encodedStream = /* Any readable stream. */)
using (Stream decodedStream = /* Any writeable stream. */)
using (var encodeStream = new UUEncodeStream(encodedStream, unixLineEnding))
{
    decodedStream.CopyTo(encodeStream);
    // Encoded contents are now in encodedStream.
}
```