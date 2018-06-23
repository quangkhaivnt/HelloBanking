using System;

namespace HelloBanking.error
{
    public class SpringHeroTransactionException: Exception
    {
        public SpringHeroTransactionException(string message) : base(message)
        {
            
        }
    }
}