using NblWebCommon;

namespace HomeSecureApi
{
    public static class UtilAuth
    {
        
        public static void VerifyUtilAuth(this HsConfig config, string key)
        {
            VerifyUtilAuth(config?.UtilAuthKey,key);
        }
        
        public static void VerifyUtilAuth(string utilAuthKey, string key)
        {
            if(string.IsNullOrWhiteSpace(utilAuthKey)){
                throw new InvalidConfigException("UtilAuthKey not set");
            }

            if(key!=utilAuthKey){
                throw new UnauthorizedException();
            }
        }
        
        public static void VerifyClientToken(this HsConfig config, string clientToken)
        {
            VerifyClientToken(config?.ClientToken,clientToken);
        }
        
        public static void VerifyClientToken(string configToken, string clientToken)
        {
            if(string.IsNullOrWhiteSpace(configToken)){
                throw new InvalidConfigException("ClientToken not set");
            }

            if(configToken!=clientToken){
                throw new UnauthorizedException();
            }
        }

    }
}