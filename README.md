# Clothing E-commerce Platform

A full-stack e-commerce web application for selling clothing products, built with ASP.NET Core Web API backend and React frontend.

## 🌟 Features

### Core Functionality
- **Product Management**: Full CRUD operations (Create, Read, Update, Delete)
- **Product Catalog**: Browse all available products
- **Product Details**: View detailed product information
- **Search & Filter**: Search products by name and description
- **Pagination**: Navigate through product pages efficiently

### API Features
- `GET /api/products` - List all products with search and pagination
- `GET /api/products/:id` - Get a single product
- `POST /api/products` - Create a new product
- `PUT /api/products/:id` - Update a product
- `DELETE /api/products/:id` - Delete a product

### UI Features
- **Responsive Design**: Mobile-friendly interface
- **Modern Styling**: Bright yellow and orange color scheme
- **Navigation Menu**: Easy navigation between pages
- **Form Validation**: Client and server-side validation
- **Error Handling**: User-friendly error messages
- **Loading States**: Visual feedback for user actions

## 🛠️ Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0 Web API
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Authentication**: Built-in validation

### Frontend
- **Framework**: React 18
- **Routing**: React Router DOM
- **HTTP Client**: Axios
- **Styling**: Custom CSS with CSS Grid and Flexbox

### DevOps
- **Containerization**: Docker
- **Version Control**: Git

## 📁 Project Structure

```
QE180063_Ass1/
├── ECommerceApp.API/           # Backend API
│   ├── Controllers/            # API Controllers
│   │   └── ProductsController.cs
│   ├── Data/                   # Database Context
│   │   └── ApplicationDbContext.cs
│   ├── Models/                 # Data Models
│   │   └── Product.cs
│   ├── Migrations/             # Database Migrations
│   └── Program.cs              # Application Entry Point
├── client/                     # Frontend React App
│   ├── public/                 # Public Assets
│   │   ├── index.html
│   │   └── manifest.json
│   └── src/                    # Source Code
│       ├── components/         # React Components
│       │   ├── HomePage.js
│       │   ├── Navbar.js
│       │   ├── ProductCard.js
│       │   ├── ProductDetail.js
│       │   ├── ProductForm.js
│       │   ├── SearchBar.js
│       │   └── Pagination.js
│       ├── services/           # API Services
│       │   └── api.js
│       ├── App.js              # Main App Component
│       └── index.js            # React Entry Point
├── Dockerfile                  # Docker Configuration
├── .dockerignore              # Docker Ignore Rules
└── README.md                  # Project Documentation
```

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Docker](https://www.docker.com/) (optional)

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd QE180063_Ass1
   ```

2. **Setup Backend**
   ```bash
   cd ECommerceApp.API
   dotnet restore
   dotnet ef database update  # If using migrations
   dotnet run
   ```
   Backend will run on `http://localhost:5000`

3. **Setup Frontend**
   ```bash
   cd client
   npm install
   npm start
   ```
   Frontend will run on `http://localhost:3000`

4. **Setup Database**
   - Install PostgreSQL
   - Create a database named `ECommerceDb`
   - Update connection string in `appsettings.json`

### Docker Deployment

1. **Build and run with Docker**
   ```bash
   docker build -t ecommerce-app .
   docker run -p 3000:3000 -p 5000:5000 ecommerce-app
   ```

2. **Access the application**
   - Frontend: `http://localhost:3000`
   - Backend API: `http://localhost:5000/api/products`
   - Swagger UI: `http://localhost:5000/swagger`

## 📊 Database Schema

### Product Model
```csharp
public class Product
{
    public int Id { get; set; }              // Primary Key
    public string Name { get; set; }         // Required, Max 100 chars
    public string Description { get; set; }  // Required, Max 1000 chars
    public decimal Price { get; set; }       // Required, > 0
    public string? Image { get; set; }       // Optional URL
    public DateTime CreatedAt { get; set; }  // Auto-generated
    public DateTime UpdatedAt { get; set; }  // Auto-updated
}
```

## 🎨 Design Features

### Color Scheme
- **Primary**: Bright Yellow (#FFEB3B)
- **Secondary**: Orange (#FF9800)
- **Accent**: Light Yellow variants
- **Text**: Dark Gray (#333)

### UI Components
- **Cards**: Product display with hover effects
- **Buttons**: Gradient backgrounds with hover animations
- **Forms**: Rounded inputs with focus states
- **Navigation**: Sticky header with active states

## 🔧 Available Scripts

### Backend
```bash
dotnet build          # Build the project
dotnet run            # Run in development mode
dotnet publish        # Build for production
dotnet ef migrations add <name>  # Add migration
dotnet ef database update       # Update database
```

### Frontend
```bash
npm start            # Start development server
npm run build        # Build for production
npm test             # Run tests
npm run eject        # Eject from Create React App
```

## 🌐 API Documentation

### Endpoints

#### Get All Products
- **GET** `/api/products`
- **Query Parameters**: `search`, `page`, `pageSize`
- **Response**: Array of products with pagination headers

#### Get Product by ID
- **GET** `/api/products/{id}`
- **Response**: Single product object

#### Create Product
- **POST** `/api/products`
- **Body**: Product object (without ID)
- **Response**: Created product with ID

#### Update Product
- **PUT** `/api/products/{id}`
- **Body**: Complete product object
- **Response**: Updated product

#### Delete Product
- **DELETE** `/api/products/{id}`
- **Response**: Success message

## 🚀 Deployment

### Docker Deployment
1. Build the Docker image: `docker build -t ecommerce-app .`
2. Run the container: `docker run -p 5000:5000 ecommerce-app`
3. Access the application at `http://localhost:5000`

### Environment Variables
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000
ConnectionStrings__DefaultConnection=<your-database-connection-string>
```

## 🧪 Testing

### Manual Testing Checklist
- [ ] Create a new product
- [ ] View product details
- [ ] Edit existing product
- [ ] Delete product
- [ ] Search for products
- [ ] Navigate between pages
- [ ] Responsive design on mobile

### API Testing
- Use Swagger UI at `/swagger` endpoint
- Test all CRUD operations
- Verify validation rules
- Check error handling

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📝 License

This project is for educational purposes as part of an assignment.

## 👥 Authors

- **Student ID**: QE180063
- **Assignment**: E-Commerce Platform Development

## 📞 Support

If you encounter any issues:
1. Check the console for error messages
2. Verify database connection
3. Ensure all dependencies are installed
4. Review the API endpoints in Swagger UI

---

**Note**: This is a student project created as part of an e-commerce platform development assignment. The application demonstrates full-stack development skills using modern web technologies.
