const { env } = require('process');

//const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
//  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:7232';

const PROXY_CONFIG = [
  {
    context: [
      "/Account/GetAccountBalance",
      "/Account/GetCustomerInfo",
      "/Account/ProcessTransaction",
    ],
    target: 'https://localhost:7232',
    secure: false
  }
]

module.exports = PROXY_CONFIG;
