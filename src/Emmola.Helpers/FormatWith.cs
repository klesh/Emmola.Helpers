﻿using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Emmola.Helpers
{
  /// <summary>
  /// modified from http://www.hanselman.com/blog/CommentView.aspx?guid=fde45b51-9d12-46fd-b877-da6172fe1791
  /// </summary>
  public static class FormatWithExtension
  {
    public static string FormatWith(this string self, object source)
    {
      return self.FormatWith(source, null);
    }

    public static string FormatWith(this string self, object source,  IFormatProvider formatProvider)
    {
      StringBuilder sb = new StringBuilder();
      Type type = source.GetType();
      Regex reg = new Regex(@"({)([^}]+)(})", RegexOptions.IgnoreCase);
      MatchCollection mc = reg.Matches(self);
      int startIndex = 0;
      foreach (Match m in mc)
      {
        Group g = m.Groups[2]; //it's second in the match between { and }
        int length = g.Index - startIndex - 1;
        sb.Append(self.Substring(startIndex, length));

        string toGet = String.Empty;
        string toFormat = String.Empty;
        int formatIndex = g.Value.IndexOf(":"); //formatting would be to the right of a :
        if (formatIndex == -1) //no formatting, no worries
        {
          toGet = g.Value;
        }
        else //pickup the formatting
        {
          toGet = g.Value.Substring(0, formatIndex);
          toFormat = g.Value.Substring(formatIndex + 1);
        }

        //first try properties
        PropertyInfo retrievedProperty = type.GetProperty(toGet);
        Type retrievedType = null;
        object retrievedObject = null;
        if (retrievedProperty != null)
        {
          retrievedType = retrievedProperty.PropertyType;
          retrievedObject = retrievedProperty.GetValue(source, null);
        }
        else //try fields
        {
          FieldInfo retrievedField = type.GetField(toGet);
          if (retrievedField != null)
          {
            retrievedType = retrievedField.FieldType;
            retrievedObject = retrievedField.GetValue(source);
          }
        }

        if (retrievedType != null && retrievedObject != null) //Cool, we found something
        {
          string result = String.Empty;
          if (toFormat == String.Empty) //no format info
          {
            result = retrievedType.InvokeMember("ToString",
              BindingFlags.Public | BindingFlags.NonPublic |
              BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase
              , null, retrievedObject, null) as string;
          }
          else //format info
          {
            result = retrievedType.InvokeMember("ToString",
              BindingFlags.Public | BindingFlags.NonPublic |
              BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase
              , null, retrievedObject, new object[] { toFormat, formatProvider }) as string;
          }
          sb.Append(result);
        }
        else //didn't find a property with that name, so be gracious and put it back
        {
          sb.Append("{");
          sb.Append(g.Value);
          sb.Append("}");
        }
        startIndex = g.Index + g.Length + 1;
      }
      if (startIndex < self.Length) //include the rest (end) of the string
      {
        sb.Append(self.Substring(startIndex));
      }
      return sb.ToString();
    }
  }
}
