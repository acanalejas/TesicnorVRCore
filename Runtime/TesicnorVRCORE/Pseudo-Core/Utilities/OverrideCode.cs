using System.IO;
using System.Text;
using System.Linq;
using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

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
    /// Ads a field to a class.
    /// Needs a FileStream with access read and write
    /// </summary>
    /// <param name="stream">FileStream of the class</param>
    /// <param name="fieldName">Desired name of the field</param>
    /// <param name="fieldValue">Desired value of the field (example: 2)</param>
    /// <param name="className">Name of the Type we want to inyect the field</param>
    /// <param name="fieldType">Type of the field example : int</param>
    /// <param name="_public">Do we want the field to be public?</param>
    public static void AddField(FileStream stream, string fieldName, string fieldValue, string className, string fieldType, bool _public)
    {
        if (!stream.CanWrite || !stream.CanRead) return;
        if (fieldName == "" || fieldName == null || fieldValue == "" || fieldValue == null || fieldType == "" || fieldType == null) return;

        string value = fieldValue != "" ? " = " + fieldValue : "";
        string field = (_public ? "public" : "private") + " " + fieldType + " " + fieldName + value + ";";
        char[] buffer = new char[stream.Length + field.Length];

        StreamReader sr = new StreamReader(stream);
        sr.ReadBlock(buffer, 0, (int)stream.Length);

        if (SearchWordInCharArray(fieldName, buffer) > 0) return;

        int wordIndex = SearchWordInCharArray(className, buffer);

        if (wordIndex <= 0) return;

        int writeIndex = SearchSymbolEndClass(wordIndex, buffer);

        char[] preBuffer = new char[writeIndex];

        for (int i = 0; i < writeIndex; i++)
        {
            preBuffer[i] = buffer[i];
        }

        char[] inyectedCode = field.ToCharArray();

        char[] postBuffer = new char[buffer.Length - writeIndex];

        for (int i = writeIndex; i < buffer.Length; i++)
        {
            postBuffer[i - writeIndex] = buffer[i];
        }

        char[] result = new char[preBuffer.Length + inyectedCode.Length + postBuffer.Length];

        for (int i = 0; i < preBuffer.Length; i++)
        {
            result[i] = preBuffer[i];
        }
        for (int i = 0; i < inyectedCode.Length; i++)
        {
            result[i + writeIndex] = inyectedCode[i];
        }
        for (int i = 0; i < postBuffer.Length; i++)
        {
            result[i + writeIndex + inyectedCode.Length] = postBuffer[i];
        }

        string result_str = "";

        foreach (var c in result) if (c != char.MinValue) result_str += c.ToString();

        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(result_str));
        ms.CopyTo(stream);
        stream.SetLength(0);
        stream.Write(Encoding.UTF8.GetBytes(result_str), 0, result_str.Length);
    }

    /// <summary>
    /// Adds a method to a class.
    /// Needs a FileStream with access read and write
    /// </summary>
    /// <param name="stream">Stream already opened from the file</param>
    /// <param name="methodName">Name of the method to create</param>
    /// <param name="methodContent">Content of the method</param>
    public static void AddMethod(FileStream stream, string methodName, string methodContent, string className, string inputType = "")
    {
        if (!stream.CanWrite || !stream.CanRead) return;
        if (methodName == "" || methodName == null || methodContent == "" || methodContent == null) return;

        string inputName = "";
        if (inputType != "") inputName = " input";
        string _method = "public void " + methodName + "(" + inputType + " " + inputName + ") \n " +
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
    /// Ads input code to the desired method
    /// </summary>
    /// <param name="stream">File stream of the class file</param>
    /// <param name="methodName">Name of the method to modify</param>
    /// <param name="newContent">Content that is going to be added</param>
    /// <param name="className">Name of the class that contains the method</param>
    public static void AddCodeToMethod(FileStream stream, string methodName, string newContent, string className)
    {
        if (methodName == null || methodName.Length == 0 || className == null || className.Length == 0 || newContent == null || newContent.Length == 0) return;
        if(!stream.CanWrite || !stream.CanRead) return;
        string content = newContent;
        if (!content.Contains(";")) content += ";";

        //15 kb buffer to read
        char[] buffer = new char[stream.Length + content.Length];

        //Reads the file and throw it to the buffer
        StreamReader sr = new StreamReader(stream);
        sr.ReadBlock(buffer, 0, (int)stream.Length);

        if (SearchWordInCharArray(methodName, buffer) <= 0) return;

        int startIndex = SearchWordInCharArray(methodName, buffer);
        int writeIndex = SearchSymbolEndClass(startIndex, buffer);

        char[] preBuffer = new char[writeIndex];

        for (int i = 0; i < writeIndex; i++)
        {
            preBuffer[i] = buffer[i];
        }

        char[] inyectedCode = content.ToCharArray();

        char[] postBuffer = new char[buffer.Length - writeIndex];

        for (int i = writeIndex; i < buffer.Length; i++)
        {
            postBuffer[i - writeIndex] = buffer[i];
        }

        char[] result = new char[preBuffer.Length + inyectedCode.Length + postBuffer.Length];

        for (int i = 0; i < preBuffer.Length; i++)
        {
            result[i] = preBuffer[i];
        }
        for (int i = 0; i < inyectedCode.Length; i++)
        {
            result[i + writeIndex] = inyectedCode[i];
        }
        for (int i = 0; i < postBuffer.Length; i++)
        {
            result[i + writeIndex + inyectedCode.Length] = postBuffer[i];
        }

        string result_str = "";

        foreach (var c in result) if (c != char.MinValue) result_str += c.ToString();

        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(result_str));
        ms.CopyTo(stream);
        stream.SetLength(0);
        stream.Write(Encoding.UTF8.GetBytes(result_str), 0, result_str.Length);
    }

    public static void AddCodeToField(FileStream stream, string fieldName, string SetContent, string GetContent, string className, bool addParameter = true, string parameterType = "")
    {
        if (fieldName == "" || fieldName == null || className == "" || className == null) return;
        if (!stream.CanWrite || !stream.CanRead) return;

        char[] buffer = new char[stream.Length];

        StreamReader sr = new StreamReader(stream);
        sr.ReadBlock(buffer, 0, (int)stream.Length);

        int afterNameIndex = SearchWordInCharArray(fieldName, buffer);

        char[] prebuffer = new char[afterNameIndex];

        for(int i = 0; i < prebuffer.Length; i++)
        {
            prebuffer[i] = buffer[i];
        }

        string content = "";
        if(SetContent != "" && SetContent != null)
        {
            content = "{ set{" + SetContent + "}";
        }
        else if(GetContent != null && GetContent != "")
        {
            content = "{";
        }
        if (GetContent != "" && GetContent != null)
        {
            content += "get{" + GetContent + "}}";
        }
        else if (content.Length > 0) 
        {
            content += "}";
        }
        if (addParameter)
        {
            string name = fieldName;
            if (fieldName.Contains(" ")) name = fieldName.Split(' ')[1];
            content += "\n \n private " + parameterType + " " + "_" + name + ";";
        }

        if (SearchWordInCharArray(content, buffer) > 0) return;

        char[] inyected = content.ToCharArray();

        char[] postBuffer = new char[buffer.Length - afterNameIndex];
        for(int i = afterNameIndex; i < buffer.Length; i++)
        {
            postBuffer[i - afterNameIndex] = buffer[i];
        }

        if(content.Length > 0)
        {
            if (postBuffer[0] == ';') postBuffer[0] = ' ';
            else if (postBuffer[1] == ';') postBuffer[1] = ' ';
            else if (postBuffer[2] == ';') postBuffer[2] = ' ';
        }

        string result = "";
        foreach (var c in prebuffer) if(c != char.MinValue) result += c;
        foreach (var c in inyected) if (c != char.MinValue) result += c;
        foreach (var c in postBuffer) if (c != char.MinValue) result += c;

        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
        ms.CopyTo(stream);
        stream.SetLength(0);
        stream.Write(Encoding.UTF8.GetBytes(result), 0, result.Length);
    }

    /// <summary>
    /// Busca una palabra en un array de caracteres y devuelve el índice de la siguiente posicion a la palabra
    /// </summary>
    /// <param name="word"></param>
    /// <param name="array"></param>
    /// <returns></returns>
    private static int SearchWordInCharArray(string word, char[] array, int offset = 0)
    {
        bool[] bools = new bool[word.Length];
        int actualBool;

        if (array == null || word == null || word == "" || array.Length <= 0) return 0;

        for(int i = offset; i < array.Length; i++)
        {
            if(array[i] == word[0])
            {
                if(i > 0)
                {
                    if (array[i - 1] != '"' || array[i - 2] != '"')
                    {
                        bools[0] = true;
                        for (int h = 1; h < word.Length; h++)
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

#if UNITY_EDITOR
    /// <summary>
    /// Devuelve el directorio donde se encuentra el objeto que le pasamos
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string GetTypePath(UnityEngine.Object obj)
    {
        string[] paths = AssetDatabase.GetAllAssetPaths();

        foreach(var path in paths)
        {
            if (path.Contains(obj.GetType().Name)) return path;
        }
        return "";
    }
#endif
#endregion
}