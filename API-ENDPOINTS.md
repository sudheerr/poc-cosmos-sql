# API Endpoints Documentation

## Overview

This API provides CRUD operations for Products, Customers, and Orders using Cosmos DB with full LINQ support.

Base URL: `https://localhost:7xxx/api`

## Products API (`/api/products`)

### Get All Products
```http
GET /api/products
```

**Response:**
```json
[
  {
    "id": "123",
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 1299.99,
    "category": "Electronics",
    "stock": 10,
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

### Get Product by ID
```http
GET /api/products/{id}
```

### Get Products by Category
```http
GET /api/products/category/{category}
```

**Example:**
```http
GET /api/products/category/Electronics
```

### Get Products in Stock
```http
GET /api/products/in-stock
```

### Search Products
```http
GET /api/products/search?searchTerm={term}&minPrice={min}&maxPrice={max}
```

**Parameters:**
- `searchTerm` (optional): Search in name or description
- `minPrice` (optional): Minimum price filter
- `maxPrice` (optional): Maximum price filter

**Example:**
```http
GET /api/products/search?searchTerm=laptop&minPrice=1000&maxPrice=2000
```

### Create Product
```http
POST /api/products
Content-Type: application/json

{
  "name": "New Product",
  "description": "Product description",
  "price": 99.99,
  "category": "Electronics",
  "stock": 50,
  "isActive": true
}
```

### Update Product
```http
PUT /api/products/{id}
Content-Type: application/json

{
  "name": "Updated Product",
  "description": "Updated description",
  "price": 149.99,
  "category": "Electronics",
  "stock": 30,
  "isActive": true
}
```

### Delete Product
```http
DELETE /api/products/{id}
```

---

## Customers API (`/api/customers`)

### Get All Customers
```http
GET /api/customers
```

**Response:**
```json
[
  {
    "id": "456",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "address": "123 Main St",
    "city": "New York",
    "country": "USA",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

### Get Customer by ID
```http
GET /api/customers/{id}
```

### Get Customer by Email
```http
GET /api/customers/email/{email}
```

**Example:**
```http
GET /api/customers/email/john@example.com
```

### Get Customers by Country
```http
GET /api/customers/country/{country}
```

**Example:**
```http
GET /api/customers/country/USA
```

### Search Customers
```http
GET /api/customers/search?searchTerm={term}
```

**Example:**
```http
GET /api/customers/search?searchTerm=john
```

### Get Active Customers (LINQ Example)
```http
GET /api/customers/active
```

### Create Customer
```http
POST /api/customers
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@example.com",
  "phoneNumber": "+1234567890",
  "address": "456 Oak Ave",
  "city": "Los Angeles",
  "country": "USA"
}
```

### Update Customer
```http
PUT /api/customers/{id}
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com",
  "phoneNumber": "+1234567890",
  "address": "456 Oak Ave",
  "city": "Los Angeles",
  "country": "USA"
}
```

### Delete Customer
```http
DELETE /api/customers/{id}
```

---

## Orders API (`/api/orders`)

### Get All Orders
```http
GET /api/orders
```

**Response:**
```json
[
  {
    "id": "789",
    "customerId": "456",
    "orderDate": "2024-01-15T10:30:00Z",
    "totalAmount": 1599.98,
    "status": "Pending",
    "items": [
      {
        "productId": "123",
        "productName": "Laptop",
        "quantity": 1,
        "unitPrice": 1299.99,
        "totalPrice": 1299.99
      },
      {
        "productId": "124",
        "productName": "Mouse",
        "quantity": 10,
        "unitPrice": 29.99,
        "totalPrice": 299.90
      }
    ],
    "createdAt": "2024-01-15T10:30:00Z"
  }
]
```

### Get Order by ID
```http
GET /api/orders/{id}
```

### Get Orders by Customer ID
```http
GET /api/orders/customer/{customerId}
```

**Example:**
```http
GET /api/orders/customer/456
```

### Get Orders by Status
```http
GET /api/orders/status/{status}
```

**Status values:** `Pending`, `Processing`, `Shipped`, `Delivered`, `Cancelled`

**Example:**
```http
GET /api/orders/status/Pending
```

### Get Pending Orders
```http
GET /api/orders/pending
```

### Get Orders by Date Range
```http
GET /api/orders/date-range?startDate={start}&endDate={end}
```

**Example:**
```http
GET /api/orders/date-range?startDate=2024-01-01&endDate=2024-01-31
```

### Get Total Revenue
```http
GET /api/orders/revenue?startDate={start}&endDate={end}
```

**Parameters:**
- `startDate` (optional): Start date for revenue calculation
- `endDate` (optional): End date for revenue calculation

**Response:**
```json
{
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-01-31T23:59:59Z",
  "totalRevenue": 15000.50,
  "currency": "USD"
}
```

### Get High Value Orders (LINQ Example)
```http
GET /api/orders/high-value?minAmount={amount}
```

**Parameters:**
- `minAmount` (optional, default: 1000): Minimum order amount

**Example:**
```http
GET /api/orders/high-value?minAmount=5000
```

### Create Order
```http
POST /api/orders
Content-Type: application/json

{
  "customerId": "456",
  "items": [
    {
      "productId": "123",
      "productName": "Laptop",
      "quantity": 1,
      "unitPrice": 1299.99,
      "totalPrice": 1299.99
    },
    {
      "productId": "124",
      "productName": "Mouse",
      "quantity": 2,
      "unitPrice": 29.99,
      "totalPrice": 59.98
    }
  ]
}
```

**Note:** `totalAmount`, `orderDate`, and `status` are calculated/set automatically.

### Update Order Status
```http
PATCH /api/orders/{id}/status
Content-Type: application/json

"Shipped"
```

### Update Order
```http
PUT /api/orders/{id}
Content-Type: application/json

{
  "customerId": "456",
  "status": "Processing",
  "items": [
    {
      "productId": "123",
      "productName": "Laptop",
      "quantity": 1,
      "unitPrice": 1299.99,
      "totalPrice": 1299.99
    }
  ]
}
```

### Delete Order
```http
DELETE /api/orders/{id}
```

---

## Common Response Codes

- `200 OK` - Request succeeded
- `201 Created` - Resource created successfully
- `204 No Content` - Delete succeeded
- `400 Bad Request` - Invalid request data
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

## LINQ Query Examples

All repositories support LINQ queries:

### Using Query() Method
```csharp
// In your code
var products = _repository.Query()
    .Where(p => p.Price > 100)
    .Where(p => p.IsActive)
    .OrderBy(p => p.Name)
    .ToList();
```

### Using FindAsync()
```csharp
var products = await _repository.FindAsync(
    p => p.Category == "Electronics" && p.Stock > 0
);
```

### Using QueryAsync() for Complex Queries
```csharp
var orders = await _repository.QueryAsync(query =>
    query.Where(o => o.TotalAmount > 1000)
         .Where(o => o.Status == "Completed")
         .OrderByDescending(o => o.OrderDate)
);
```

## Testing with cURL

### Create a Product
```bash
curl -X POST https://localhost:7001/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Gaming Laptop",
    "description": "High-end gaming laptop",
    "price": 1999.99,
    "category": "Electronics",
    "stock": 5
  }'
```

### Get All Products
```bash
curl https://localhost:7001/api/products
```

### Search Products
```bash
curl "https://localhost:7001/api/products/search?searchTerm=laptop&minPrice=1000"
```

### Create a Customer
```bash
curl -X POST https://localhost:7001/api/customers \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "address": "123 Main St",
    "city": "New York",
    "country": "USA"
  }'
```

### Create an Order
```bash
curl -X POST https://localhost:7001/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "customer-id-here",
    "items": [
      {
        "productId": "product-id-here",
        "productName": "Laptop",
        "quantity": 1,
        "unitPrice": 1299.99,
        "totalPrice": 1299.99
      }
    ]
  }'
```

## Swagger UI

Access the interactive API documentation at:
```
https://localhost:7xxx/swagger
```

The Swagger UI provides:
- Interactive API testing
- Request/response examples
- Schema definitions
- Authentication testing

---

**All endpoints support Cosmos DB partitioning and LINQ queries!** 🚀
