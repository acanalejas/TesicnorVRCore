using System.IO;
using System.Text;
using System.Linq;
using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public static class OverrideCode 
{
    #region PARAMETERS

    #endregion

    #region FUNCTIONS
    /// <summary>
    /// Opens an only write stream
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static FileStream WriteStream(string path)
    {
        return File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
    }

    /// <summary>
    /// Opens an only read stream
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static FileStream ReadStream(string path)
    {
        return File.Open(path, FileMode.OpenOrCreate, FileAccess.Read);
    }

    /// <summary>
    /// Opens a stream in Write and Read modes
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static FileStream BothStream(string path)
    {
        return File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
    }

    public static void CloseFileStream(FileStream stream)
    {
        stream.Close();
    }

    /// <summary>
    /// Adds a method to a class.
    /// Needs a FileStream with access read and write
    /// </summary>
    /// <param name="stream">Stream already opened from the file</param>
    /// <param name="methodName">Name of the method to create</param>
    /// <param name="methodContent">Content of the method</param>
    public static void AddMethod(FileStream stream, string methodName, string methodContent, string className)
    {
        if (!stream.CanWrite || !stream.CanRead) return;
        if (methodName == "" || methodName == null || methodContent == "" || methodContent == null) return;

        string _method = "public void " + methodName + "() \n " +
            "{\n" +
               "   " + methodContent +
               "\n }";
        
        //15 kb buffer to read
       char[] buffer = new char[stream.Length + _method.Length];

        //Reads the file and throw it to the buffer
        StreamReader sr = new StreamReader(stream);
        sr.ReadBlock(buffer, 0, (int)stream.Length);

        if (SearchWordInCharArray(methodName, buffer) > 0) return;

        string readen = "";

        //Puts all the chars into a string
        foreach(char c in buffer)
        {
            readen += c.ToString();
        }

        int wordIndex = SearchWordInCharArray(className, buffer);

        if (wordIndex <= 0) return;

        int writeIndex = SearchSymbolEndClass(wordIndex, buffer);

        char[] preBuffer = new char[writeIndex];

        for(int i = 0; i < writeIndex; i++)
        {
            preBuffer[i] = buffer[i];
        }

        char[] inyectedCode = _method.ToCharArray();

        char[] postBuffer = new char[buffer.Length - writeIndex];

        for(int i = writeIndex; i < buffer.Length; i++)
        {
            postBuffer[i - writeIndex] = buffer[i];
        }

        char[] result = new char[preBuffer.Length + inyectedCode.Length + postBuffer.Length];
        
        for(int i = 0; i < preBuffer.Length; i++)
        {
            result[i] = preBuffer[i];
        }
        for(int i = 0; i < inyectedCode.Length; i++)
        {
            result[i + writeIndex] = inyectedCode[i];
        }
        for(int i = 0; i < postBuffer.Length; i++)
        {
            result[i + writeIndex + inyectedCode.Length] = postBuffer[i];
        }

        string result_str = "";
        
        foreach (var c in result) if(c != char.MinValue)result_str += c.ToString();

        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(result_str));
        ms.CopyTo(stream);
        stream.SetLength(0);
        stream.Write(Encoding.UTF8.GetBytes(result_str), 0, result_str.Length);
    }

    /// <summary>
    /// Busca una palabra en un array de caracteres y devuelve el índice de la siguiente posicion a la palabra
    /// </summary>
    /// <param name="word"></param>
    /// <param name="array"></param>
    /// <returns></returns>
    private static int SearchWordInCharArray(string word, char[] array)
    {
        bool[] bools = new bool[word.Length];
        int actualBool;

        if (array == null || word == null || word == "" || array.Length <= 0) return 0;

        for(int i = 0; i < array.Length; i++)
        {
            if(array[i] == word[0])
            {
                bools[0] = true;
                for(int h = 1; h < word.Length; h++)
                {
                    if (array[i + h] == word[h])
                    {
                        bools[h] = true;
                    }
                    else
                    {
                        for (int j = 0; j < bools.Length; j++) bools[j] = false;
                        break;
                    }
                }
            }

            bool allTrue = true;
            foreach(bool b in bools)
            {
                if (!b) { allTrue = false; break; }
            }

            if (allTrue)
            {
                return i + word.Length;
            }
            
        }
        return 0;
    }

    /// <summary>
    /// Busca el ultimo corchete de la clase y devuelve su posicion
    /// </summary>
    /// <param name="startIndex">Posicion del inicio de la clase</param>
    /// <param name="array">Array de caracteres</param>
    /// <returns>Posicion del ultimo corchete de la clase</returns>
    private static int SearchSymbolEndClass(int startIndex, char[] array)
    {
        int x = 0;
        int index = 0;
        foreach (char c in array)
        {
            if (c == '{') x++;
            if (c == '}') { x--; if (x <= 0) return index; }
            index++;
        }

        return 0;
    }
    #endregion
}
#endif