using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CN_SharePoint_Library_Syn
{
    public static class SPHelper
    {
        private static ClientContext context = null;

        public static bool AddFileToSPLib(string fullFileName, string webUrl, string libName)
        {
            try
            {
                SPAccount account = Global.SPAccounts().FirstOrDefault(s => s.SPSite.ToLower() == webUrl.ToLower());

                if (account == null)
                {
                    Global.WriteLog("There is no SPAccount entry for " + webUrl + " in the setting xml file.", EventLogEntryType.Error);
                    return false;
                }

                SecureString securePassword = new SecureString();
                foreach (var passwordChar in account.Password)
                {
                    securePassword.AppendChar(passwordChar);
                }
                using (context = new ClientContext(webUrl))
                {
                    context.Credentials = new SharePointOnlineCredentials(account.UserName, securePassword);
                    context.Load(context.Web, w => w.Title);
                    context.ExecuteQuery();

                    Microsoft.SharePoint.Client.ListCollection collList = context.Web.Lists;

                    context.LoadQuery(collList.Include(
                           list => list.Title,
                           list => list.Id,
                           list => list.Hidden,
                           list => list.BaseType));

                    context.Load(collList);

                    context.ExecuteQuery();

                    var targetList = collList.FirstOrDefault(l => l.Title.ToLower() == libName.ToLower());

                    string fileName = System.IO.Path.GetFileName(fullFileName);

                    fileName = Global.CleanInvalidCharacters(fileName);

                    context.Load(targetList);
                    context.ExecuteQuery();

                    context.Load(targetList.RootFolder);
                    context.ExecuteQuery();

                    Microsoft.SharePoint.Client.File currentFile = null;

                    var tempcurrentFile = CheckIfItemAlreadyExistByDisplayName(fileName, targetList);
                    if (tempcurrentFile != null && tempcurrentFile.Count() != 0)
                    {
                        currentFile = tempcurrentFile.FirstOrDefault().File;
                        if (targetList.ForceCheckout)
                            currentFile.CheckOut();
                    }

                    using (FileStream fs = new FileStream(fullFileName, FileMode.Open))
                    {
                        string targetLocation = targetList.RootFolder.ServerRelativeUrl + "/" + fileName;
                        Microsoft.SharePoint.Client.File.SaveBinaryDirect(context, targetLocation, fs, true);
                    }

                    if (targetList.ForceCheckout)
                        currentFile.CheckIn("Updated on " + DateTime.Now, CheckinType.MajorCheckIn);
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("it is being used by another process"))
                    Global.WriteLog(string.Format("Can not add the file {0} in the SP lib {1}. Reason: ",
                        fullFileName,
                        libName,
                        ex.Message), EventLogEntryType.Error);
                return false;
            }
        }

        internal static bool DeleteFileToSPLib(string fileName, string webUrl, string libName)
        {
            try
            {
                SPAccount account = Global.SPAccounts().FirstOrDefault(s => s.SPSite.ToLower() == webUrl.ToLower());

                if (account == null)
                {
                    Global.WriteLog("There is no SPAccount entry for " + webUrl + " in the setting xml file.", EventLogEntryType.Error);
                    return false;
                }
                using (context = new ClientContext(webUrl))
                {
                    SecureString securePassword = new SecureString();
                    foreach (var passwordChar in account.Password)
                    {
                        securePassword.AppendChar(passwordChar);
                    }
                    context.Credentials = new SharePointOnlineCredentials(account.UserName, securePassword);
                    context.Load(context.Web, w => w.Title);
                    context.ExecuteQuery();

                    Microsoft.SharePoint.Client.ListCollection collList = context.Web.Lists;

                    context.LoadQuery(collList.Include(
                           list => list.Title,
                           list => list.Id,
                           list => list.Hidden,
                           list => list.BaseType));

                    context.Load(collList);

                    context.ExecuteQuery();

                    var targetList = collList.FirstOrDefault(l => l.Title.ToLower() == libName.ToLower());

                    var existingFile = CheckIfItemAlreadyExistByDisplayName(fileName, targetList);

                    if (existingFile != null && existingFile.Count() != 0)
                    {
                        var existingItem = existingFile.FirstOrDefault();
                        context.Load(existingItem);
                        context.ExecuteQuery();
                        existingItem.DeleteObject();
                        context.ExecuteQuery();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Global.WriteLog(string.Format("Can not delete the file {0} in the SP lib {1}. Reason: ",
                    fileName,
                    libName,
                    ex.Message), EventLogEntryType.Error);
                return false;
            }
        }

        internal static bool RenameFileToSPLib(string oldFileName, string newFileName, string webUrl, string libName)
        {
            try
            {
                SPAccount account = Global.SPAccounts().FirstOrDefault(s => s.SPSite.ToLower() == webUrl.ToLower());

                if (account == null)
                {
                    Global.WriteLog("There is no SPAccount entry for " + webUrl + " in the setting xml file.", EventLogEntryType.Error);
                    return false;
                }
                using (context = new ClientContext(webUrl))
                {
                    SecureString securePassword = new SecureString();
                    foreach (var passwordChar in account.Password)
                    {
                        securePassword.AppendChar(passwordChar);
                    }
                    context.Credentials = new SharePointOnlineCredentials(account.UserName, securePassword);
                    context.Load(context.Web, w => w.Title);
                    context.ExecuteQuery();

                    Microsoft.SharePoint.Client.ListCollection collList = context.Web.Lists;

                    context.LoadQuery(collList.Include(
                           list => list.Title,
                           list => list.Id,
                           list => list.Hidden,
                           list => list.BaseType));

                    context.Load(collList);

                    context.ExecuteQuery();

                    var targetList = collList.FirstOrDefault(l => l.Title.ToLower() == libName.ToLower());
                    context.Load(targetList.RootFolder);
                    context.ExecuteQuery();

                    var existingFile = CheckIfItemAlreadyExistByDisplayName(oldFileName, targetList);

                    if (existingFile != null && existingFile.Count() != 0)
                    {
                        var file = existingFile.FirstOrDefault();
                        context.Load(file);
                        context.ExecuteQuery();
                        string newLocation = file["FileDirRef"] + "/" +
                            Global.CleanInvalidCharacters(Path.GetFileName(newFileName));
                        file.File.MoveTo(newLocation, MoveOperations.AllowBrokenThickets);
                        context.ExecuteQuery();
                    }
                    else
                    {
                        AddFileToSPLib(newFileName, webUrl, libName);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Global.WriteLog(string.Format("Can not rename the file {0} to the new name {1} in the SP lib {2}. Reason: {3}",
                    Path.GetFileName(oldFileName),
                    Path.GetFileName(newFileName),
                    libName, ex.Message), EventLogEntryType.Error);
                return false;
            }
        }

        private static ListItemCollection CheckIfItemAlreadyExistByDisplayName(string fileName, List targetList)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);

            ListItemCollection existingItems = targetList.GetItems(CamlQuery.CreateAllItemsQuery());
            context.Load(existingItems, page => page.Include(item => item.DisplayName).Where(obj => obj.DisplayName == fileName));
            context.ExecuteQuery();
            return existingItems;
        }

    }
}
