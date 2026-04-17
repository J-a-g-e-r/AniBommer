using Unity.Netcode.Components;
using UnityEngine;

namespace Unity.Multiplayer.Samples.Utilities.ClientAuthority
{
    /// <summary>
    /// Được sử dụng để đồng bộ hóa Transform với các thay đổi từ phía Client.
    /// Bao gồm cả Host. Server thuần túy không phải là owner thì không nên sử dụng
    /// cho các transform luôn được sở hữu bởi server.
    /// </summary>
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {
        /// <summary>
        /// Xác định ai có quyền ghi vào transform này. 
        /// Trả về false nghĩa là Owner Client có quyền ghi (Client Authority).
        /// Lưu ý: Điều này đặt sự tin tưởng vào phía Client.
        /// </summary>
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}