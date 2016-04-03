using System.ServiceModel.Channels;
using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace Rafy.WCF
{
    /// <summary>
    /// A message encoder class that compacts the message size by compressing it.
    /// </summary>
    internal class CompactMessageEncoder : MessageEncoder
    {
        /// <summary>
        /// Stores the content type name of the message
        /// </summary>
        private const string _contentType = "application/x-gzip";

        /// <summary>
        /// Holds the inner message encoder (binary or text)
        /// </summary>
        private readonly MessageEncoder _innerEncoder;

        /// <summary>
        /// Initialize the message encoder with an inner encoder
        /// </summary>
        /// <param name="innerEncoder">Binary or Text message encoder to use as an inner encoder</param>
        public CompactMessageEncoder(MessageEncoder innerEncoder)
        {
            _innerEncoder = innerEncoder;
        }

        /// <summary>
        /// Get the content type of the message
        /// </summary>
        public override string ContentType
        {
            get { return _contentType; }
        }

        /// <summary>
        /// Get the media type of the encoder
        /// </summary>
        public override string MediaType
        {
            get { return _innerEncoder.MediaType; }
        }

        /// <summary>
        /// Get the message version of the encoder
        /// </summary>
        public override MessageVersion MessageVersion
        {
            get { return _innerEncoder.MessageVersion; }
        }

        /// <summary>
        /// Decompress and desearialize array of bytes into a message. 
        /// </summary>
        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            ArraySegment<byte> decompressedBuffer = DecompressBuffer(buffer, bufferManager);

            LogWrite("Decompressed from {0} bytes to {1} bytes", buffer.Count, decompressedBuffer.Count);

            Message returnMessage = _innerEncoder.ReadMessage(decompressedBuffer, bufferManager);
            returnMessage.Properties.Encoder = this;
            return returnMessage;
        }

        /// <summary>
        /// Searialize and compress a message into an array of bytes
        /// </summary>
        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            ArraySegment<byte> buffer = _innerEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
            ArraySegment<byte> compressedBuffer = CompressBuffer(buffer, bufferManager, messageOffset);

            LogWrite("Compressed from {0} bytes to {1} bytes", buffer.Count, compressedBuffer.Count);

            return compressedBuffer;
        }

        #region Buffer compression and decompression

        /// <summary>
        /// Compress a buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bufferManager"></param>
        /// <param name="messageOffset"></param>
        /// <returns></returns>
        private static ArraySegment<byte> CompressBuffer(ArraySegment<byte> buffer, BufferManager bufferManager, int messageOffset)
        {
            // Create a memory stream for the final message
            MemoryStream memoryStream = new MemoryStream();

            // Copy the bytes that should not be compressed into the stream
            memoryStream.Write(buffer.Array, 0, messageOffset);

            // Compress the message into the stream
            using (GZipStream gzStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gzStream.Write(buffer.Array, messageOffset, buffer.Count);
            }

            // Convert the stream into a bytes array
            byte[] compressedBytes = memoryStream.ToArray();

            // Allocate a new buffer to hold the new bytes array
            byte[] bufferedBytes = bufferManager.TakeBuffer(compressedBytes.Length);

            // Copy the compressed data into the allocated buffer
            Array.Copy(compressedBytes, 0, bufferedBytes, 0, compressedBytes.Length);

            // Release the original buffer we got as an argument
            bufferManager.ReturnBuffer(buffer.Array);

            // Create a new ArraySegment that points to the new message buffer
            ArraySegment<byte> byteArray = new ArraySegment<byte>(bufferedBytes, messageOffset, compressedBytes.Length - messageOffset);

            return byteArray;
        }

        /// <summary>
        /// Decompress a buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bufferManager"></param>
        /// <returns></returns>
        private static ArraySegment<byte> DecompressBuffer(ArraySegment<byte> buffer, BufferManager bufferManager)
        {
            // Create a new memory stream, and copy into it the buffer to decompress
            MemoryStream memoryStream = new MemoryStream(buffer.Array, buffer.Offset, buffer.Count);

            // Create a memory stream to store the decompressed data
            MemoryStream decompressedStream = new MemoryStream();

            // The totalRead stores the number of decompressed bytes
            int totalRead = 0;

            int blockSize = 1024;

            // Allocate a temporary buffer to use with the decompression
            byte[] tempBuffer = bufferManager.TakeBuffer(blockSize);

            // Uncompress the compressed data
            using (GZipStream gzStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                while (true)
                {
                    // Read from the compressed data stream
                    int bytesRead = gzStream.Read(tempBuffer, 0, blockSize);
                    if (bytesRead == 0)
                        break;
                    // Write to the decompressed data stream
                    decompressedStream.Write(tempBuffer, 0, bytesRead);
                    totalRead += bytesRead;
                }
            }

            // Release the temporary buffer
            bufferManager.ReturnBuffer(tempBuffer);

            // Convert the decompressed data stream into bytes array
            byte[] decompressedBytes = decompressedStream.ToArray();

            // Allocate a new buffer to store the message 
            byte[] bufferManagerBuffer = bufferManager.TakeBuffer(decompressedBytes.Length + buffer.Offset);

            // Copy the bytes that comes before the compressed message in the buffer argument
            Array.Copy(buffer.Array, 0, bufferManagerBuffer, 0, buffer.Offset);

            // Copy the decompressed data
            Array.Copy(decompressedBytes, 0, bufferManagerBuffer, buffer.Offset, decompressedBytes.Length);

            // Create a new ArraySegment that points to the new message buffer
            ArraySegment<byte> byteArray = new ArraySegment<byte>(bufferManagerBuffer, buffer.Offset, decompressedBytes.Length);

            // Release the original message buffer
            bufferManager.ReturnBuffer(buffer.Array);

            return byteArray;
        }

        #endregion

        #region 不支持的操作

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            throw new NotSupportedException("Compression Encoding is not yet supported for streamed communications");
            //Message message = _innerEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);
            //return message;
        }

        public override void WriteMessage(Message message, Stream stream)
        {
            throw new NotSupportedException("Compression Encoding is not yet supported for streamed communications");
            //_innerEncoder.WriteMessage(message, stream);
        }

        #endregion

        [Conditional("LOG")]
        private void LogWrite(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}