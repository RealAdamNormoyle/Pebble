using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using SimpleJSON;

public class ServerManager : MonoBehaviour {

    public static ServerManager Instance;

    public string DataUrl = "http://amnsoftware.co.uk/pebble/pebble-actions.php";

    public void Awake()
    {
        Instance = this;
    }

    public void MakeServerRequest<T>(Action<JSONNode> onSuccess, Action<string> onError, T data)
    {
        StartCoroutine(WaitForWWWResponse(onSuccess, onError, data));
    }

    IEnumerator WaitForWWWResponse<T>(Action<JSONNode> onSuccess, Action<string> onError, T data)
    {
        WWWForm form = new WWWForm();

        foreach (var member in data.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public))
        {
            if (member.MemberType == MemberTypes.Property)
                AddField(form, member.Name, (member as PropertyInfo).GetValue(data, null));
            else if (member.MemberType == MemberTypes.Field)
                AddField(form, member.Name, (member as FieldInfo).GetValue(data));
        }

        WWW response = new WWW(DataUrl, form);

        yield return response;

        //LogF("Response received: {0}", response.text);

        if (!string.IsNullOrEmpty(response.error))
        {
            if (onError != null)
                onError(response.error);
        }
        else
        {
            try
            {


            var json = JSON.Parse(response.text);

            if (json["result"].Value == "ok")
            {
                if (onSuccess != null)
                    onSuccess(json["data"]);
            }
            else
            {
                if (onError != null)
                    onError(json["message"]);
            }
            }catch(Exception e)
            {
                Debug.Log(e);
            }
           
        }
    }

    public void AddField(WWWForm form, string field, object value)
    {
        if (field == "binary")
        {
            var binaryField = value as BinaryField;
            if (binaryField != null)
                form.AddBinaryData(field, binaryField.data, binaryField.filename, binaryField.mimeType);
        }
        else
        {
            form.AddField(field, value.ToString());
        }
    }
    public string Combine(string url1, string url2)
    {
        if (url1.Length == 0)
            return url2;

        if (url2.Length == 0)
            return url1;

        url1 = url1.TrimEnd('/', '\\');
        url2 = url2.TrimStart('/', '\\');

        return string.Format("{0}/{1}", url1, url2);
    }


}

public class BinaryField
{
    public byte[] data;
    public string filename;
    public string mimeType;

    static Dictionary<string, string> m_mimeTypes = new Dictionary<string, string> {
        { "doc", "application/msword" },
        { "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { "gif", "image/gif" },
        { "jpeg", "image/jpeg" },
        { "jpg", "image/jpeg" },
        { "pdf", "application/pdf" },
        { "png", "image/png" }
    };

    public BinaryField(byte[] data, string filename, string path)
    {
        var extension = Path.GetExtension(path).ToLower().TrimStart('.');
        var mimeType = m_mimeTypes[extension];

        this.data = data;
        this.filename = filename;
        this.mimeType = mimeType;
    }
}

public class AsyncResult
{
    public bool success;
    public string errorMessage;

    public AsyncResult()
    {
        success = true;
    }

    public void Error(string errorMessage)
    {
        success = false;
        this.errorMessage = errorMessage;
    }
}
