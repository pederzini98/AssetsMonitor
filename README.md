# AssetsMonitor
AssetsMonitor
Implementing a console application which will look for some especific asset in the https://brapi.dev free api
There is some values that must be passed in the App.config file that must be located inside the 'ConsoleApplication' project
Inside this file the user must pass his smtp server configs, and the recepients that will receive the email.
In the command line or Debug variables, the user must pass the name of the assets, a value to sell it and a value to buy Ex: PETR4 1000 25
Every request to the https://brapi.dev that are higher or low than the ones defined will trigger an email notification to the declared users in the app.config.

I will kepp working on this project because i want to implemment an clean architecture using a webAPI and PostgreSQl, The next steps will be to configure MediatR
to help with the Dependecy Injection.
