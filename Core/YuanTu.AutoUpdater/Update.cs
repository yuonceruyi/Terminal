using System;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace YuanTu.AutoUpdater
{
    public static class Update
    {
        public static bool Check()
        {
            IAutoUpdater autoUpdater = new AutoUpdater();
            try
            {
                return autoUpdater.Check();
            }
            catch (Exception)
            {
                //Log the message to your file or database
                return false;
            }
        }

        public static void Do()
        {
            #region check and download new version program

            var bHasError = false;
            IAutoUpdater autoUpdater = new AutoUpdater();
            try
            {
                autoUpdater.Update();
            }
            catch (WebException ex)
            {
                CommonUnitity.log(ex.Message + "\n" + ex.StackTrace);
                MessageBox.Show("Can not find the specified resource");
                bHasError = true;
            }
            catch (XmlException ex)
            {
                bHasError = true;
                CommonUnitity.log(ex.Message + "\n" + ex.StackTrace);
                MessageBox.Show("Download the upgrade file error");
            }
            catch (NotSupportedException ex)
            {
                bHasError = true;
                CommonUnitity.log(ex.Message + "\n" + ex.StackTrace);
                MessageBox.Show("Upgrade address configuration error");
            }
            catch (ArgumentException ex)
            {
                bHasError = true;
                CommonUnitity.log(ex.Message + "\n" + ex.StackTrace);
                MessageBox.Show("Download the upgrade file error");
            }
            catch (Exception ex)
            {
                bHasError = true;
                CommonUnitity.log(ex.Message + "\n" + ex.StackTrace);
                MessageBox.Show("An error occurred during the upgrade process");
            }
            finally
            {
                if (bHasError)
                {
                    try
                    {
                        autoUpdater.RollBack();
                    }
                    catch (Exception ex)
                    {
                        //Log the message to your file or database
                        CommonUnitity.log(ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }

            #endregion check and download new version program
        }
    }
}