using System;
using System.Collections;
using Unity.Services.Core;
using Unity.Services.Core.Internal;
using UnityEngine;
using AsyncOperation = Unity.Services.Core.Internal.AsyncOperation;
using Logger = Unity.Services.Authentication.Utilities.Logger;

namespace Unity.Services.Authentication
{
    class AuthenticationAsyncOperation : IAsyncOperation
    {
        AsyncOperation m_AsyncOperation;
        RequestFailedException m_Exception;

        public AuthenticationAsyncOperation()
        {
            m_AsyncOperation = new AsyncOperation();
            m_AsyncOperation.SetInProgress();
        }

        /// <summary>
        /// Complete the operation as a failure.
        /// </summary>
        public void Fail(int errorCode, string message, Exception innerException = null)
        {
            Fail(AuthenticationException.Create(errorCode, message, innerException));
        }

        /// <summary>
        /// Complete the operation as a failure with the exception.
        /// </summary>
        /// <remarks>
        /// Exception with type other than <see cref="RequestFailedException"/> are wrapped as
        /// an <see cref="RequestFailedException"/> with error code <code>CommonErrorCodes.Unknown</code>.
        /// </remarks>
        public void Fail(Exception exception)
        {
            if (exception is RequestFailedException)
            {
                m_Exception = (RequestFailedException)exception;
            }
            else
            {
                m_Exception = new RequestFailedException(CommonErrorCodes.Unknown, exception.Message, exception);
            }
            Logger.LogException(m_Exception);

            BeforeFail?.Invoke(this);
            m_AsyncOperation.Fail(m_Exception);
        }

        /// <summary>
        /// Complete this operation as a success.
        /// </summary>
        public void Succeed()
        {
            m_AsyncOperation.Succeed();
        }

        /// <summary>
        /// The event to invoke in case of failure right before marking the operation done.
        /// This is a good place to put some cleanup code before sending out the completed callback.
        /// </summary>
        public event Action<AuthenticationAsyncOperation> BeforeFail;

        /// <inheritdoc/>
        public bool IsDone
        {
            get => m_AsyncOperation.IsDone;
        }

        /// <inheritdoc/>
        public AsyncOperationStatus Status
        {
            get => m_AsyncOperation.Status;
        }

        /// <inheritdoc/>
        public event Action<IAsyncOperation> Completed
        {
            add => m_AsyncOperation.Completed += value;
            remove => m_AsyncOperation.Completed -= value;
        }

        /// <summary>
        /// The exception that occured during the operation if it failed.
        /// The value can be set before the operation is done.
        /// </summary>
        public RequestFailedException Exception
        {
            get => m_Exception;
        }

        /// <inheritdoc/>
        Exception IAsyncOperation.Exception
        {
            get => m_Exception;
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() => !IsDone;

        /// <inheritdoc/>
        /// <remarks>
        /// Left empty because we don't support operation reset.
        /// </remarks>
        void IEnumerator.Reset() {}

        /// <inheritdoc/>
        object IEnumerator.Current => null;
    }
}
