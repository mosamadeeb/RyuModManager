using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Utils
{
    public class GameHash
    {
        public static bool ValidateFile(string path, Game game)
        {
            using MD5 md5Hash = MD5.Create();
            using FileStream file = File.OpenRead(path);
            var gameHashes = GetValidGameHashes(game);
            byte[] exeHash = md5Hash.ComputeHash(file);
            return gameHashes.Length == 0
                || (from x in gameHashes 
                    where x.SequenceEqual(exeHash) 
                    select x).Any();
        }

        private static byte[][] GetValidGameHashes(Game game)
        {
            return game switch
            {
                Game.Yakuza3 => new byte[][] { 
                    new byte[] { 172, 112, 65, 90, 116, 185, 119, 107, 139, 148, 48, 80, 40, 13, 107, 113 } 
                },
                Game.Yakuza4 => new byte[][] { 
                    new byte[] { 41, 89, 36, 15, 180, 25, 237, 66, 222, 176, 78, 130, 33, 146, 77, 132 } 
                },
                Game.Yakuza5 => new byte[][] { 
                    new byte[] { 51, 96, 128, 207, 98, 131, 90, 216, 213, 88, 198, 186, 60, 99, 176, 201 } 
                },
                Game.Yakuza0 => new byte[][] { 
                    new byte[] { 168, 70, 120, 237, 170, 16, 229, 118, 232, 54, 167, 130, 194, 37, 220, 14 }, // Steam ver.
                    new byte[] { 32, 44, 24, 38, 67, 27, 82, 26, 205, 131, 3, 24, 44, 150, 150, 84 }          // GOG ver.
                },
                Game.YakuzaKiwami => new byte[][] { 
                    new byte[] { 142, 39, 38, 133, 251, 26, 47, 181, 222, 56, 98, 207, 178, 123, 175, 8 }, // Steam ver.
                    new byte[] { 114, 65, 77, 21, 216, 176, 138, 129, 56, 13, 182, 66, 10, 202, 126, 150 } //GOG ver.
                },
                Game.Yakuza6 => new byte[][] { 
                    new byte[] { 176, 204, 180, 91, 160, 163, 81, 217, 243, 92, 5, 157, 214, 129, 217, 7 } 
                },
                Game.YakuzaKiwami2 => new byte[][] { 
                    new byte[] { 143, 2, 192, 39, 60, 179, 172, 44, 242, 201, 155, 226, 50, 192, 204, 0 },   // Steam ver.
                    new byte[] { 193, 175, 140, 27, 230, 27, 94, 96, 67, 221, 175, 168, 32, 228, 240, 101 }  // GOG ver.
                },
                Game.YakuzaLikeADragon => new byte[][] { 
                    new byte[] { 188, 204, 133, 1, 251, 100, 190, 56, 10, 122, 164, 173, 244, 134, 246, 5 } 
                },
                _ => new byte[][] { },
            };
        }
    }
}
