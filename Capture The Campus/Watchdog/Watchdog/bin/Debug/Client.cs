﻿using System;
using System.IO;
using System.Net.Sockets;

namespace Watchdog
{
    public class Client
    {
        //This pair of variables holds the IP address of the server and the port of the server globally for easy access
        private static string ipAddress = "localhost";
        private static int port = 43;

        //These are used to store the variables that will bypassed from the client to the server
        private static string username;
        private static string location;

        public string Input(string[] args)
        {
            //These are used to store variables used to verify the arguments given to the client
            int noOfArgs = 0;
            bool argBool = false;

            //This calls the method used to verify the arguments given
            FromUser(args, ref noOfArgs, ref argBool);

            //If the arguments given are valid this calls the method which updates the server
            if (argBool)
            {
                return ToTCPServer(args, ref noOfArgs);
            }
            else
            {
                return null;
            }
        }

        private static void FromUser(string[] args, ref int noOfArgs, ref bool argBool)
        {
            //If there are no arguments the program doesn't bother to execute any further code
            if (args.Length != 0)
            {
                //These are used to track the progress through the assignment process and to pass the correct arguments to the correct variables
                int i = 0;
                bool usernameBool = false;
                bool locationBool = false;

                //This is used to exit the program if the arguments given are invalid
                argBool = true;

                //This ensures the correct number of arguments are initialized
                while (i < args.Length)
                {
                    //This tests to see if a user name has already been set
                    if (usernameBool == true)
                    {
                        //This sets the location
                        location = args[i];

                        //This ensures the program does not crash by executing the following lines on a empty string
                        if (location != string.Empty)
                        {
                            //This removes all black space from the beginning and end of the location before putting it into an array of its characters
                            char[] locationCharArray = location.Trim().ToCharArray();

                            //if the location begins with a / the location is made invalid so as not to interfere with future code
                            if ((locationCharArray[0] == '/'))
                            {
                                argBool = false;

#if DEBUG
                                Console.WriteLine("ERROR: Invalid arguments.");
#endif

                                //This breaks out of the case
                                break;
                            }
                        }

                        //If the location is already set that must mean that an invalid set of arguments has been entered as the last argument will always be the location thus this portion of code will exit the program under these circumstances
                        if (locationBool == true)
                        {
                            argBool = false;

#if DEBUG
                            Console.WriteLine("ERROR: Invalid arguments.");
#endif

                            //This breaks out of the case
                            break;
                        }
                        else
                        {
                            //This is a boolean to track the status of the location
                            locationBool = true;
                        }
                    }
                    else
                    {
                        //This sets the user name
                        username = args[i];

                        //This is a boolean to track the status of the user name
                        usernameBool = true;
                    }

                    //This is used to break out of the enclosing while loop
                    i++;

                    //This breaks out of the while loop if the arguments given are invalid
                    if (!argBool)
                    {
                        break;
                    }
                }
            }
            else
            {
#if DEBUG
                //This prints to the screen an error message
                Console.WriteLine("ERROR: No arguments given.");
#endif
            }
        }

        private static string ToTCPServer(string[] args, ref int noOfArgs)
        {
            //These variables are used to store messages to be sent to the server and screen
            string toInput = string.Empty;
            string toOutput = string.Empty;

            //This variable dictates if the output from the server needs to be read or not
            bool serverOutput = false;

            //If the number of arguments unaccounted for is one then there must only be arguments for a user name
            if (args.Length - noOfArgs == 1)
            {
                //This indicates that the server will return something of value
                serverOutput = true;

                //This addes things to be outputted to the screen
                toOutput = username + ": ";

                //This sets the string to query the server
                toInput = username;
            }

            //If the number of arguments unaccounted for is one then there must be arguments for a user name and a location
            else
            {
                if (args.Length - noOfArgs == 2)
                {
                    //This addes things to be outputted to the screen
                    toOutput = username + " location changed to be " + location;

                    //This sets the string to query the server
                    toInput = username + " " + location;
                }
                else
                {
                    toOutput = "ERROR: arguments invalid";
                }
            }

            try
            {
                if (toInput != string.Empty)
                {
                    #region Client

                    //This initializes a new Tcp client to handle the requests to the server
                    TcpClient client = new TcpClient();

                    client.Connect(ipAddress, port);

                    //These are the variables used to pass data too and from the server
                    StreamWriter streamWriter = new StreamWriter(client.GetStream());
                    StreamReader streamReader = new StreamReader(client.GetStream());

                    //This ensures that if the server hangs for too long the client doesn't wait for a response
                    client.SendTimeout = 3000;

                    //This sends the queries to the database
                    streamWriter.WriteLine(toInput);
                    streamWriter.Flush();

                    //This ensures that if the server hangs for too long the client doesn't wait for a response
                    client.ReceiveTimeout = 3000;

                    #endregion

                    #region ToScreenOutput

                    //This will skip the response code if the response from the server is not needed
                    if (serverOutput)
                    {
                        //This continues while there are lines to read
                        while ((client.Connected) && (!streamReader.EndOfStream) && (streamReader.Peek() != -1))
                        {
                            //This reads the next line from the server
                            string nextLine = streamReader.ReadLine();

                            //This adds it to the variable to be printed to the screen
                            toOutput = toOutput + nextLine;

                            //This is used to ensure a stack overflow is not caused
                            if (nextLine == null)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        //This ensures the client doesn't crash when the server is trying to force a response that is unneeded
                        while ((client.Connected) && (!streamReader.EndOfStream) && (streamReader.Peek() != -1))
                        {
                            streamReader.ReadLine();
                        }
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                //This prints to the screen an error message
                toOutput = "ERROR: " + ex.ToString();
            }

#if DEBUG
            //This prints the server response to the screen
            Console.WriteLine(toOutput);
#endif

            return toOutput;
        }
    }
}