﻿using Eth.Json;
using Eth.Rpc;
using Eth.Utilities;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Eth
{
    public class IpcClient : BaseClient, IDisposable
    {
        private static string DefaultPipePath
        {
            get { return "geth.ipc"; }
        }

        private static IJsonSerializer DefaultJsonSerializer
        {
            get { return new JsonSerializer(); }
        }

        protected readonly IJsonSerializer _jsonSerializer;
        protected readonly NamedPipeClientStream _pipeStream;

        /// <summary>
        /// Creates an IpcClient with pipe as the default pipe name.
        /// </summary>
        public IpcClient() : this(DefaultPipePath, DefaultJsonSerializer) { }

        /// <summary>
        /// Creates an IpcClient pointing at pipe specified by the pipename
        /// </summary>
        /// <param name="pipeName">Name of the pipe. eg. "geth.ipc"</param>
        public IpcClient(string pipeName) : this(pipeName, DefaultJsonSerializer) { }

        /// <summary>
        /// Creates an IpcClient pointing at pipe specified by the pipename with custom serializer
        /// </summary>
        /// <param name="pipeName">Name of the pipe. eg. "geth.ipc"</param>
        /// <param name="jsonSerializer">Serializer used to serialize/deserialize the json returned by httpClient</param>
        public IpcClient(string pipeName, IJsonSerializer jsonSerializer)
        {
            Ensure.EnsureStringIsNotNullOrEmpty(pipeName, "pipeName");
            Ensure.EnsureParameterIsNotNull(jsonSerializer, "jsonSerialzer");

            _pipeStream = new NamedPipeClientStream(pipeName);
            _pipeStream.Connect();
            _jsonSerializer = jsonSerializer;
        }

        public override async Task<RpcResponse<T>> PostRpcRequestAsync<T>(RpcRequest request)
        {
            string jsonRequest = _jsonSerializer.Serialize(request);
            Debug.WriteLine(String.Format("Serialized Request: {0}", jsonRequest));

            byte[] bytes = Encoding.UTF8.GetBytes(jsonRequest);
            await _pipeStream.WriteAsync(bytes, 0, bytes.Length);

            byte[] buff = new byte[_pipeStream.OutBufferSize];
            await _pipeStream.ReadAsync(buff, 0, buff.Length);

            string jsonResponse = Encoding.UTF8.GetString(buff);

            Debug.WriteLine(String.Format("Serialized Response: {0}", jsonResponse));

            RpcError rpcError = _jsonSerializer.Deserialize<RpcError>(jsonResponse);

            if (rpcError.Error != null)
            {
                throw new EthException(rpcError.Error.ErrorCode, rpcError.Error.Message);
            }

            RpcResponse<T> rpcResponse = _jsonSerializer.Deserialize<RpcResponse<T>>(jsonResponse);

            return rpcResponse;
        }

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                if (_pipeStream != null) _pipeStream.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
