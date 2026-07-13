using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.AOP.CallTrace
{
    /// <summary>
    /// 调用链追踪上下文
    /// 使用 AsyncLocal 在异步调用链中传递 TraceId 和调用层级
    /// </summary>
    public static class CallTraceContext
    {
        private static readonly AsyncLocal<CallTraceState> _current = new();

        /// <summary>
        /// 当前调用链状态
        /// </summary>
        public static CallTraceState Current
        {
            get
            {
                if (_current.Value == null)
                {
                    _current.Value = new CallTraceState
                    {
                        TraceId = Guid.NewGuid().ToString("N")[..8],
                        Depth = 0
                    };
                }
                return _current.Value;
            }
            set => _current.Value = value;
        }

        /// <summary>
        /// 开始一个新的调用链
        /// </summary>
        public static void StartNewTrace()
        {
            _current.Value = new CallTraceState
            {
                TraceId = Guid.NewGuid().ToString("N")[..8],
                Depth = 0
            };
        }

        /// <summary>
        /// 进入子调用
        /// </summary>
        public static IDisposable EnterScope()
        {
            Current.Depth++;
            return new ScopeToken();
        }

        private class ScopeToken : IDisposable
        {
            public void Dispose()
            {
                Current.Depth--;
            }
        }
    }

}
