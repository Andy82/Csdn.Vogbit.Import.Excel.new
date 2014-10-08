using System;
using System.Runtime.Remoting;
using com.imageright.auth;
using com.imageright.connection;
using imageright.interfaces;
using ImageRight.Client;

namespace ImgrAutochecker
{
    class ConnectToImgr
    {
        public ImageRightSystem Connect()
        {
            ImageRightSystem _irSystem;
            RemotingConfiguration.Configure("c:\\Program Files (x86)\\ImageRight\\Clients\\imageright.emc.exe.config", false);
            ConnectionManager.ConfigureRemoting();
            //ConnectionInfo connectionInfo = new ConnectionInfo(0, "IMGR 5.6 SP1", "tcp://LOCALHOST:8082");
            ConnectionInfo connectionInfo = new ConnectionInfo(0, "master", "tcp://LOCALHOST:8082");

            UserCredentials uc = new UserCredentials("Admin", "Admin");
            connectionInfo.ClientAuthCallback = new SimpleCredentialsProvider(uc);

            System.Threading.Thread.SetData(System.Threading.Thread.GetNamedDataSlot("CREDENTIALS"), uc);
            IConnection connection = ConnectionManager.ConnectionFactory.CreateConnection(connectionInfo);
            _irSystem = new ImageRightSystem(connection);
            if (!IsValidUser(_irSystem))
                throw new ImageRight.Client.Exception("Please check username and password.");

            return _irSystem;
        }

        private bool IsValidUser(ImageRightSystem _irSystem)
        {
            try
            {
                _irSystem.Connections[0].Security.GetCurrentUser();
            }
            catch (ImageRight.Client.Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return false;
            }
            return true;
        }
    }
}
