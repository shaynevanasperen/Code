using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Extensions.Net
{
	static partial class DnsEndPointExtension
	{
		/// <summary>
		/// Establishes whether or not connectivity can be made to the given <see cref="DnsEndPoint"/>.
		/// Throws a more helpful exception if the connection attempt fails. This can be used to alleviate
		/// the dreaded "No such host is known." error message when a DNS lookup fails, by returning a message
		/// stating what hostname was attempted.
		/// </summary>
		/// <param name="endPoint">The endpoint to attempt a dummy socket connection.</param>
		/// <param name="name">The name to use in the error message, in case connection failed.</param>
		/// <param name="timeout">How long to wait for connection attempt before giving up.</param>
		/// <param name="socketType">The <see cref="SocketType"/> to use for the dummy connection (defaults to <see cref="SocketType.Stream"/>).</param>
		/// <param name="protocolType">The <see cref="ProtocolType"/> to use for the dummy connection (defaults to <see cref="ProtocolType.Tcp"/>).</param>
		/// <returns>A task representing the ongoing connection attempt.</returns>
		internal static async Task AssertConnectivityAsync(this DnsEndPoint endPoint, string name, TimeSpan timeout,
			SocketType socketType = SocketType.Stream, ProtocolType protocolType = ProtocolType.Tcp)
		{
			using (var socketAsyncEventArgs = new ConnectEventArgs())
			{
				using (var cancellationTokenSource = new CancellationTokenSource(timeout))
				{
					try
					{
						socketAsyncEventArgs.Initialize(cancellationTokenSource.Token);
						socketAsyncEventArgs.RemoteEndPoint = endPoint;

						if (Socket.ConnectAsync(socketType, protocolType, socketAsyncEventArgs))
						{
							// Connect completing asynchronously. Enable it to be canceled and wait for it.
							using (cancellationTokenSource.Token.Register(state => Socket.CancelConnectAsync((SocketAsyncEventArgs)state), socketAsyncEventArgs))
							{
								await socketAsyncEventArgs.Builder.Task.ConfigureAwait(false);
							}
						}
						else if (socketAsyncEventArgs.SocketError != SocketError.Success)
						{
							// Connect completed synchronously but unsuccessfully.
							throw new SocketException((int)socketAsyncEventArgs.SocketError);
						}
					}
					catch (OperationCanceledException e)
					{
						throw new System.Exception($"Connection attempt to {name} host '{endPoint.Host}:{endPoint.Port}' timed out after {timeout.Seconds} seconds.", e);
					}
					catch (System.Exception e)
					{
						throw new System.Exception($"Connection attempt to {name} host '{endPoint.Host}:{endPoint.Port}' failed.", e);
					}
				}
			}
		}

		/// <summary>
		/// SocketAsyncEventArgs that carries with it additional state for a Task builder and a CancellationToken.
		/// </summary>
		private sealed class ConnectEventArgs : SocketAsyncEventArgs
		{
			public AsyncTaskMethodBuilder Builder { get; private set; }
			private CancellationToken CancellationToken { get; set; }

			public void Initialize(CancellationToken cancellationToken)
			{
				CancellationToken = cancellationToken;
				AsyncTaskMethodBuilder b = default(AsyncTaskMethodBuilder);
				_ = b.Task; // force initialization
				Builder = b;
			}

			protected override void OnCompleted(SocketAsyncEventArgs _)
			{
				switch (SocketError)
				{
					case SocketError.Success:
						Builder.SetResult();
						break;

					case SocketError.OperationAborted:
					case SocketError.ConnectionAborted:
						if (CancellationToken.IsCancellationRequested)
						{
							Builder.SetException(new OperationCanceledException(CancellationToken));
							break;
						}
						goto default;

					default:
						Builder.SetException(new SocketException((int)SocketError));
						break;
				}
			}
		}
	}
}
