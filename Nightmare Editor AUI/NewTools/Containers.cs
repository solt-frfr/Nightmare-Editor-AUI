using System;
using System.IO;
using System.Runtime.CompilerServices;
using MsBox.Avalonia;

namespace Nightmare_Editor.NewTools;

public class Containers
{
    public class Generic
    {
        public static void Pack(string file)
        {
            
            try
            {
                if (Path.GetExtension(file).ToLower() == ".l2d")
                {
                    L2D.L2D.ReplaceAllTextures(file);
                }
                if (Path.GetExtension(file).ToLower() == ".pmo")
                {
                    PMO.ReplaceAllTextures(file);
                }
                if (Path.GetExtension(file).ToLower() == ".pmp")
                {
                    PMP.ReplaceAllTextures(file);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void Unpack(string file)
        {
            if (Path.GetExtension(file).ToLower() == ".l2d")
            {
                L2D.L2D.ExtractAllTextures(file);
            }
            if (Path.GetExtension(file).ToLower() == ".pmo")
            {
                PMO.ExtractAllTextures(file);
            }
            if (Path.GetExtension(file).ToLower() == ".pmp")
            {
                PMP.ExtractAllTextures(file);
            }
        }
    }
    
    public static bool IsArc(string file)
    {
        if (Path.GetExtension(file) == ".pmo" || Path.GetExtension(file) == ".l2d" || Path.GetExtension(file) == ".fep" || Path.GetExtension(file) == ".pmp")
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}