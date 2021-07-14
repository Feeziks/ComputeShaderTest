namespace ComputeShaderUtility
{
    using UnityEngine;

    public static class ComputeUtils
    {

        //Public Methods

        // Create a buffer without emplacing any data
        /// <summary>
        /// Create a Structured Compute Buffer of given count with sizeof type T, without emplacing any data
        /// </summary>
        /// <typeparam name="T"> Type of data to be used in ComputeBuffer</typeparam>
        /// <param name="buff"> Reference to the ComputeBuffer. Any non-null Buffer will be release and new buffer will be returned.</param>
        /// <param name="count"> Number of items ComputeBuffer will be sized to accept</param>
        public static void CreateStructuredBuffer<T>(ref ComputeBuffer buff, int count)
        {
            bool createNewBuffer = buff == null || !buff.IsValid() || buff.count != count || buff.stride != SizeOfBytes<T>();
            if(createNewBuffer)
            {
                if (buff != null)
                    buff.Release();
                buff = new ComputeBuffer(count, SizeOfBytes<T>());
            }
        }

        /// <summary>
        /// Create a Structured ComputeBuffer of a given count and data size without emplacing any data
        /// </summary>
        /// <param name="buff">Reference to the ComputeBuffer. Any non-null Buffer will be release and new buffer will be returned.</param>
        /// <param name="count">Number of items ComputeBuffer will be sized to accept</param>
        /// <param name="size">Size in Bytes of the data type this Buffer will accept</param>
        public static void CreateStructuredBuffer(ref ComputeBuffer buff, int count, int size)
        {
            bool createNewBuffer = buff == null || !buff.IsValid() || buff.count != count || buff.stride != size;
            if (createNewBuffer)
            {
                if(buff != null)
                    buff.Release();
                buff = new ComputeBuffer(count, size);
            }
        }

        //Create a buffer an emplace the given data

        /// <summary>
        /// Create a structured ComputeBuffer and sets the passed data
        /// </summary>
        /// <typeparam name="T">Data type this buffer will support</typeparam>
        /// <param name="buff">Reference to the ComputeBuffer to create and emplace data</param>
        /// <param name="data">Data to place into the ComputeBuffer</param>
        public static void CreateAndSetStructuredBuffer<T>(ref ComputeBuffer buff, T[] data)
        {
            CreateStructuredBuffer<T>(ref buff, data.Length);
            buff.SetData(data);
        }

        public static void CreateAndSetStructuredBuffer<T>(ref ComputeBuffer buff, T[,] data)
        {
            CreateStructuredBuffer<T>(ref buff, data.GetLength(0) * data.GetLength(1));
            buff.SetData(data);
        }

        /// <summary>
        /// Create a structured ComputeBuffer and sets the passed data
        /// </summary>
        /// <param name="buff">Reference to the ComputeBuffer to create and emplace data</param>
        /// <param name="data">Data to place into the ComputeBuffer</param>
        /// <param name="size">Size in bytes of passed data type</param>
        public static void CreateAndSetStructuredBuffer(ref ComputeBuffer buff, Object[] data, int size)
        {
            CreateStructuredBuffer(ref buff, data.Length, size);
            buff.SetData(data);
        }

        /// <summary>
        /// Create a structured ComputeBuffer and sets the passed data
        /// </summary>
        /// <param name="buff">Reference to the ComputeBuffer to create and emplace data</param>
        /// <param name="data">Data to place into the ComputeBuffer</param>
        public static void CreateAndSetStructuredBuffer(ref ComputeBuffer buff, Object[] data)
        {
            CreateStructuredBuffer(ref buff, data.Length, SizeOfBytes(data[0]));
            buff.SetData(data);
        }

        /// <summary>
        /// Get the thread group sizes of a passed compute shader at a given kernel index
        /// </summary>
        /// <param name="shader">The compute shader to get thread group sizes from</param>
        /// <param name="kernelIndex">Kernel index from passed compute shader to get thread group sizes, default = 0</param>
        /// <returns></returns>
        public static Vector3Int GetThreadGroupSizes(ComputeShader shader, int kernelIndex = 0)
        {
            uint x = 0, y = 0, z = 0;
            shader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
            return new Vector3Int((int)x, (int)y, (int)z);
        }

        /// <summary>
        /// Create a RenderTexture of given Width and Height
        /// </summary>
        /// <param name="texture">Reference to the Texture to create</param>
        /// <param name="width">Width of the RenderTexture to create</param>
        /// <param name="height">Height of the RenderTexture to create</param>
        public static void CreateRenderTexture(ref RenderTexture texture, int width, int height)
        {
            CreateRenderTexture(ref texture, width, height, 0);
        }

        /// <summary>
        /// Create a RenderTexture of given Width and Height
        /// </summary>
        /// <param name="texture">Reference to the Texture to create</param>
        /// <param name="width">Width of the RenderTexture to create</param>
        /// <param name="height">Height of the RenderTexture to create</param>
        /// <param name="depth">Depth of the RenderTexture to create</param>
        public static void CreateRenderTexture(ref RenderTexture texture, int width, int height, int depth)
        {
            //Check if the passed texture needs to be cleared
            if (texture != null)
            {
                if(texture.IsCreated())
                    texture.Release();
            }

            if (depth != 0 && depth != 16 && depth != 24)
                throw new System.ArgumentException("Depth value must be 0, 16, or 24!");

            //Construct and create the texture
            texture = new RenderTexture(width, height, depth);
            texture.enableRandomWrite = true;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Point;
            texture.useMipMap = false;
            texture.Create();
        }

        public static void CopyToRenderTexture(Texture source, RenderTexture dest)
        {
            RenderTexture.active = dest;
            Graphics.Blit(source, dest);
            RenderTexture.active = null;
        }

        public static void CopyRenderTextureToTexture2D(RenderTexture source, Texture2D dest)
        {
            //RenderTexture prevActive = RenderTexture.active;
            RenderTexture.active = source;
            dest.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            dest.Apply();
            RenderTexture.active = null;
            //if (prevActive != null)
            //    RenderTexture.active = prevActive;
        }


        //Private Method 
        private static int SizeOfBytes<T>()
        {
            return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
        }

        private static int SizeOfBytes(object obj)
        {
            return System.Runtime.InteropServices.Marshal.SizeOf(obj);
        }


    }

}