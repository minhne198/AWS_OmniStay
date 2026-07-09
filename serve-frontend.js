const http = require('http');
const fs = require('fs');
const path = require('path');

const PORT = 5173;
const FRONTEND_DIR = path.join(__dirname, 'frontend');

const mimeTypes = {
  '.html': 'text/html',
  '.js': 'text/javascript',
  '.css': 'text/css',
  '.json': 'application/json',
  '.png': 'image/png',
  '.jpg': 'image/jpg',
  '.jpeg': 'image/jpeg',
  '.gif': 'image/gif',
  '.svg': 'image/svg+xml',
  '.ico': 'image/x-icon'
};

const server = http.createServer((req, res) => {
  let reqPath = req.url.split('?')[0];
  
  // Xử lý đường dẫn gốc
  if (reqPath === '/') {
    reqPath = '/public/index.html';
  }
  
  let filePath = path.join(FRONTEND_DIR, reqPath);
  
  // Nếu file không tồn tại, thử tìm trong /public
  if (!fs.existsSync(filePath)) {
    const publicFilePath = path.join(FRONTEND_DIR, 'public', reqPath);
    if (fs.existsSync(publicFilePath)) {
      filePath = publicFilePath;
    } else {
      // Thử tìm trong /src
      const srcFilePath = path.join(FRONTEND_DIR, 'src', reqPath.replace(/^\//, ''));
      if (fs.existsSync(srcFilePath)) {
        filePath = srcFilePath;
      }
    }
  }
  
  const extname = String(path.extname(filePath)).toLowerCase();
  const contentType = mimeTypes[extname] || 'application/octet-stream';

  fs.readFile(filePath, (error, content) => {
    if (error) {
      console.error('Error reading file:', filePath, error.code);
      if (error.code === 'ENOENT') {
        res.writeHead(404, { 'Content-Type': 'text/html' });
        res.end('<h1>404 Not Found</h1>', 'utf-8');
      } else {
        res.writeHead(500);
        res.end('Sorry, check with the site admin for error: ' + error.code + ' ..\n');
      }
    } else {
      console.log('Serving:', filePath);
      res.writeHead(200, { 'Content-Type': contentType });
      res.end(content, 'utf-8');
    }
  });
});

server.listen(PORT, () => {
  console.log(`Frontend server running at http://127.0.0.1:${PORT}/`);
  console.log(`Homepage: http://127.0.0.1:5173/`);
  console.log(`Prototype: http://127.0.0.1:${PORT}/public/prototype/index.html`);
});
