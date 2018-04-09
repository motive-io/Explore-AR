using Motive.Core.Utilities;
using System;


namespace Motive.Core
{
    class ShareTokenManager : Singleton<ShareTokenManager>
    {
        public string ParseOutToken(string uri)
        {
            var bits = uri.Split('/');
            var shareIndex = -20;

            for (var i = 0; i < bits.Length; i++)
            {
                var piece = bits[i];

                if (piece.Equals("share", StringComparison.CurrentCultureIgnoreCase))
                {
                    shareIndex = i;
                }
                else if (i == shareIndex + 2)// '/space/{appId}/{token}'
                {
                    return piece;
                }
            }
            return null;
        }

        public string ParseOutAppId(string uri)
        {
            var bits = uri.Split('/');
            var shareIndex = -20;

            for (var i = 0; i < bits.Length; i++)
            {
                var piece = bits[i];

                if (piece.Equals("share", StringComparison.CurrentCultureIgnoreCase))
                {
                    shareIndex = i;
                }
                else if (i == shareIndex + 1)// '/space/{appId}/{token}'
                {
                    return piece;
                }
            }
            return null;
        }

    }
}