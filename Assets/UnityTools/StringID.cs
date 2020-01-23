using System;
namespace UnityTools {
    public static class StringID {
        static Random rand = new Random();
        const string pool = "ABCDEFGHIJKLMNOPQRSTUVWXYabcdefghjlkmnopqrstuvwxyz0123456789";
        public static string Generate () {
            int length = rand.Next(7, 15);
            char[] output = new char[length];
            for (int i = 0; i < length; i++) 
                output[i] =  pool[rand.Next(0, pool.Length)];
            
            return new string(output);
        }
    }
}