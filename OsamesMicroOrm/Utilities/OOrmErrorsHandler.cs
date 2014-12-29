﻿/*
This file is part of OSAMES Micro ORM.
Copyright 2014 OSAMES

OSAMES Micro ORM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

OSAMES Micro ORM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with OSAMES Micro ORM.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OsamesMicroOrm.Utilities
{
    /// <summary>
    /// Boîte à outils pour la gestion des messages d'erreurs.
    /// </summary>
    public class OOrmErrorsHandler
    {
        private KeyValuePair<ErrorType, string> ErrorMsg;

        /// <summary>
        /// Dictionnaire interne des erreurs au format suivant : clé : code d'erreur "E_XXX". Valeur : code HRESULT "0x1234" et texte.
        /// </summary>
        public static readonly Dictionary<string, KeyValuePair<string, string>> HResultCode = new Dictionary<string, KeyValuePair<string, string>>();

        /// <summary>
        /// Cosntructor
        /// </summary>
        static OOrmErrorsHandler()
        {
            ReadHResultCodesFromResources("HResult Orm.csv", out HResultCode);
        }

        private static void ReadHResultCodesFromResources(string resource_, out Dictionary<string, KeyValuePair<string, string>> hresultCodes_)
        {
            using (Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(OOrmErrorsHandler).Assembly.GetName().Name + ".Resources." + resource_))
            {
                hresultCodes_ = new Dictionary<string, KeyValuePair<string, string>>();
                //null if resource doesn't exist
                if (str == null)
                    return;

                using (StreamReader sr = new StreamReader(str))
                {
                    //Skip csv header
                    sr.ReadLine();

                    string currentLine;
                    // currentLine will be null when the StreamReader reaches the end of file

                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        //Insert to kvp
                        string[] row = currentLine.Split(';');
                        if(row.Length > 0 && row.Length < 2)
                            throw new Exception("Incorrect line : needs at least E_CODE and HRESULT hexa code");
                        string eCode = row[1].Substring(1, row[1].Length - 2);
                        string hexCode = row[0].Substring(1, row[0].Length - 2);
                        if (string.IsNullOrWhiteSpace(eCode) || string.IsNullOrWhiteSpace(hexCode))
                            continue;
                        hresultCodes_.Add(eCode.ToUpperInvariant(), new KeyValuePair<string, string>(hexCode, row[2].Substring(1, row[2].Length - 2)));
                    }
                }
            }
        }
        /// <summary>
        /// return a key value pair with hresult hexa code and description
        /// </summary>
        /// <param name="code_"></param>
        /// <returns></returns>
        internal static string FindHResultByCode(HResultEnum code_)
        {
            string code = code_.ToString().ToUpperInvariant();
            return HResultCode.ContainsKey(code) ? string.Format("{0} ({1})", HResultCode[code].Value, HResultCode[code].Key) 
                                                  : string.Format("No code {0} found.", code_);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorType"></param>
        /// <param name="message_"></param>
        internal  void AddErrorMessage(ErrorType errorType, string message_)
        {
            // TODO à mon avis supprimer.
            ErrorMsg = new KeyValuePair<ErrorType, string>(errorType, DateTime.Now + " :: " + message_);
        }

        internal void DisplayErrorMessageWinforms()
        {
            System.Windows.Forms.MessageBoxIcon errorBoxIcon;
             // TODO ne plus utiliser la variable static ErroMsg mais utiliser un paramètre.
            switch (ErrorMsg.Key)
            {
                    case ErrorType.CRITICAL: errorBoxIcon = System.Windows.Forms.MessageBoxIcon.Stop; break;
                    case ErrorType.ERROR: errorBoxIcon = System.Windows.Forms.MessageBoxIcon.Error; break;
                    case ErrorType.WARNING: errorBoxIcon = System.Windows.Forms.MessageBoxIcon.Warning; break;
                    default: errorBoxIcon = System.Windows.Forms.MessageBoxIcon.None; break;
            }
            System.Windows.Forms.MessageBox.Show(ErrorMsg.Value, "ORM Message", System.Windows.Forms.MessageBoxButtons.OK, errorBoxIcon);
        }
    }

    internal enum ErrorType
    {
        // ReSharper disable InconsistentNaming
        CRITICAL,
        ERROR,
        WARNING,
        // ReSharper enable InconsistentNaming
    }
}
