import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44332/',
  redirectUri: baseUrl,
  clientId: 'ChatBot_App',
  responseType: 'code',
  scope: 'offline_access ChatBot',
  requireHttps: true,
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'ChatBot',
  },
  oAuthConfig ,
  apis: {
    default: {
      url: 'https://localhost:44332',
      rootNamespace: 'ChatBot',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;
