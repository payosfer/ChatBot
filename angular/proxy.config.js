const PROXY_CONFIG = [
  {
    context: [
      "/api",
    ],
    target: "https://localhost:44332", // Backend URL'niz
    secure: false,
    changeOrigin: true,
    logLevel: "debug"
  }
];

module.exports = PROXY_CONFIG;