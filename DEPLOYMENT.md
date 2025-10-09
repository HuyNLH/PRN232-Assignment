# Deployment Guide

## Current Setup
- **Backend + Database**: Render (https://prn232-assignment-jo3d.onrender.com)
- **Frontend**: Vercel

## Frontend (Vercel) Configuration

### Option 1: Using vercel.json (Recommended - Already Added)
The `vercel.json` file is already configured. Just push to GitHub and Vercel will auto-deploy.

### Option 2: Manual Configuration in Vercel Dashboard
If you prefer manual setup or vercel.json doesn't work:

1. Go to your Vercel project settings
2. **Build & Development Settings**:
   - **Root Directory**: Leave empty (or set to `/`)
   - **Build Command**: `cd client && npm install && npm run build`
   - **Output Directory**: `client/build`
   - **Install Command**: `cd client && npm install`

3. **Environment Variables** (Important!):
   - Add: `REACT_APP_API_URL` = `https://prn232-assignment-jo3d.onrender.com/api`
   - This overrides the .env.production file

### Vercel Environment Variable Setup
1. Go to Vercel Dashboard → Your Project → Settings → Environment Variables
2. Add the following variable:
   ```
   Name: REACT_APP_API_URL
   Value: https://prn232-assignment-jo3d.onrender.com/api
   Environment: Production
   ```
3. Click "Save"
4. Redeploy your project

## Backend (Render) Configuration

### Environment Variables Required on Render:
Make sure these are set in your Render dashboard:

```
DATABASE_URL=postgres://user:password@host:port/database
# OR individually:
DB_HOST=your-postgres-host.render.com
DB_PORT=5432
DB_NAME=your_db_name
DB_USER=your_db_user
DB_PASSWORD=your_db_password

PORT=(automatically set by Render)
ASPNETCORE_ENVIRONMENT=Production
```

### CORS Configuration
The backend now allows ALL Vercel domains (`*.vercel.app`), so any Vercel deployment URL will work automatically.

## Testing the Connection

### 1. Check Backend Health
```bash
curl https://prn232-assignment-jo3d.onrender.com/api/products
```

### 2. Check Frontend Console
Open browser DevTools → Console and look for:
- API requests being made
- Any CORS errors
- Network tab to see actual requests

### 3. Common Issues

**Issue**: CORS error in browser console
- **Solution**: Backend CORS is now configured to allow all Vercel domains. If still blocked, check that backend is deployed with latest code.

**Issue**: Frontend shows "localhost:5000" in network requests
- **Solution**: Set `REACT_APP_API_URL` environment variable in Vercel dashboard, then redeploy.

**Issue**: Backend returns 500 error
- **Solution**: Check Render logs for database connection issues. Verify DATABASE_URL is set correctly.

## Local Development

### Backend
```powershell
cd ECommerceApp.API
dotnet run
```
Backend runs on http://localhost:5000

### Frontend
```powershell
cd client
npm install
npm start
```
Frontend runs on http://localhost:3000 and proxies API calls to localhost:5000

## Deployment Checklist

### Before Deploying Backend (Render):
- [ ] Set DATABASE_URL or individual DB_* environment variables
- [ ] Push latest code to GitHub
- [ ] Verify migrations run successfully in logs

### Before Deploying Frontend (Vercel):
- [ ] Set REACT_APP_API_URL environment variable in Vercel
- [ ] Verify vercel.json is in repository root
- [ ] Push latest code to GitHub
- [ ] Check build logs for success

### After Deployment:
- [ ] Test API endpoint: https://prn232-assignment-jo3d.onrender.com/api/products
- [ ] Test frontend loads correctly
- [ ] Test frontend can fetch products from backend
- [ ] Check browser console for errors
- [ ] Check Network tab for API calls

## Files Changed for Deployment

1. **client/.env.production** - Production API URL
2. **ECommerceApp.API/Program.cs** - Fixed CORS to allow all Vercel domains
3. **vercel.json** - Vercel build configuration
4. **DEPLOYMENT.md** - This guide
