
using System;

namespace doturn
{
    static class Constants
    {
        public static class Stun
        {
            public static readonly byte[] MAGIC_COOKIE = { 33, 18, 164, 66 };
            public static readonly byte[] INVALID_TRANSACTION_ID = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }
        public static class StunMessage
        {
            public static readonly byte[] BINDING = { 0, 1 };
            public static readonly byte[] BINDING_SUCCESS = { 1, 1 };
            public static readonly byte[] ALLOCATE = { 0, 3 };
        }
        public static class StunAttr
        {
            public static readonly byte[] MAPPED_ADDRESS = { 0, 1 };
            public static readonly byte[] XOR_MAPPED_ADDRESS = { 0, 32 };
        }
    }
}