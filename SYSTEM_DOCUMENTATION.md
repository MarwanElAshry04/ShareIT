# ShareIT Application - System Documentation

## 1. Overview of Application

### Purpose
ShareIT is an enterprise employee-voice management system (formerly **SpeakUp**, which
handled complaints only). It broadens the scope to cover three kinds of submissions —
**Complaints, Suggestions, and general Feedback** — while retaining confidentiality options
and structured status tracking. Every submission is stored as a **Ticket** carrying a
`Kind` discriminator (`Complaint` | `Suggestion` | `Feedback`) chosen at submission time.

### Key Objectives
- Provide a secure channel for employees to raise complaints, suggestions, and feedback
- Classify each submission by `Kind` and enable per-kind filtering and dashboard breakdowns
- Enable transparent tracking of ticket status and timeline
- Support multi-level ticket management and follow-up
- Maintain audit trails and documentation of all tickets
- Facilitate role-based access control and permission management
- Support document and video evidence management
- Enable email-based ticket submission and notifications

### Key Functions
1. **Ticket Management**: Create, track, and manage tickets through multiple workflow stages
2. **User Authentication & Authorization**: Identity-based authentication with role-based access control (RBAC)
3. **Evidence Management**: Support for document and video file uploads
4. **Timeline Tracking**: Automatic logging of ticket workflow states and transitions
5. **Email Integration**: Support for email-based ticket submissions and notifications
6. **Search & Filtering**: Comprehensive search across tickets by section, category, and other criteria
7. **Reporter Management**: Handling of anonymous and identified reporters
8. **Ticket Type Management**: Categorization of tickets with customizable types
9. **Department & Company Management**: Multi-organizational support
10. **Role & Permission Management**: Customizable roles with granular permissions

---

## 2. Detailed Network Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         Client Layer (Web Browser)                      │
│                                                                         │
│  • Razor Pages UI (Views)                                              │
│  • HTML/CSS/JavaScript                                                │
│  • Bootstrap Framework                                                 │
└─────────────────────────────────────────────────────────────────────────┘
									│
									│ HTTP/HTTPS
									▼
┌─────────────────────────────────────────────────────────────────────────┐
│              Application Server Layer (ASP.NET Core 8)                  │
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │ Web Server (Kestrel / IIS)                                       │  │
│  │                                                                  │  │
│  │  ┌─────────────────────────────────────────────────────────┐   │  │
│  │  │ ASP.NET Core MVC Controllers & Razor Pages             │   │  │
│  │  │                                                         │   │  │
│  │  │ • HomeController (Dashboard)                          │   │  │
│  │  │ • TicketController (CRUD operations)               │   │  │
│  │  │ • VideosController (Video management)                 │   │  │
│  │  │ • DocumentController (Document management)             │   │  │
│  │  │ • UsersController (User management)                    │   │  │
│  │  │ • RolesController (Role management)                    │   │  │
│  │  │ • SearchController (Search functionality)              │   │  │
│  │  │ • CompaniesController (Organization management)        │   │  │
│  │  │ • DepartmentsController (Department management)        │   │  │
│  │  │ • TicketTypesController (Ticket categorization)  │   │  │
│  │  │ • RelationsController (Relation management)            │   │  │
│  │  │ • EmailsController (Email management)                  │   │  │
│  │  │ • FollowTicketController (Ticket tracking)       │   │  │
│  │  └─────────────────────────────────────────────────────────┘   │  │
│  │                            │                                    │  │
│  │                            ▼                                    │  │
│  │  ┌─────────────────────────────────────────────────────────┐   │  │
│  │  │ Service Layer                                           │   │  │
│  │  │                                                         │   │  │
│  │  │ • IEmailService / EmailService                         │   │  │
│  │  │ • TimelineService                                      │   │  │
│  │  │ • Authentication & Authorization Services             │   │  │
│  │  │ • File Upload/Management Services                      │   │  │
│  │  └─────────────────────────────────────────────────────────┘   │  │
│  │                            │                                    │  │
│  │                            ▼                                    │  │
│  │  ┌─────────────────────────────────────────────────────────┐   │  │
│  │  │ Entity Framework Core (ORM)                             │   │  │
│  │  │                                                         │   │  │
│  │  │ • DbContext: ApplicationDbContext                       │   │  │
│  │  │ • Model Mapping & Migrations                           │   │  │
│  │  └─────────────────────────────────────────────────────────┘   │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │ ASP.NET Core Identity                                            │  │
│  │                                                                  │  │
│  │ • User Authentication                                           │  │
│  │ • Password Management & Hashing                                 │  │
│  │ • Role-Based Access Control (RBAC)                              │  │
│  │ • Claims-Based Authorization                                    │  │
│  │ • Email Confirmation                                            │  │
│  │ • Account Lockout Management                                    │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │ File Storage Management                                          │  │
│  │                                                                  │  │
│  │ • wwwroot/uploads/videos/ (Video storage)                        │  │
│  │ • wwwroot/uploads/documents/ (Document storage)                  │  │
│  │ • wwwroot/uploads/logos/ (Logo storage)                          │  │
│  │ • wwwroot/CompFiles/ (Ticket attachments)                     │  │
│  │ • wwwroot/uploads/chat/ (Chat file uploads)                      │  │
│  └──────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
									│
					┌───────────────┴───────────────┬──────────────┐
					│ SQL                           │ SMTP Email   │
					▼                               ▼              │
	┌─────────────────────────────────────┐  ┌──────────────┐   │
	│ SQL Server Database                 │  │ Email Server │   │
	│ (10.0.10.154:1433)                  │  │ (SMTP)       │   │
	│                                     │  └──────────────┘   │
	│ Database: ShareIT_lastTest          │                     │
	│ Authentication: SQL Server Auth      │                     │
	│ (sa / encrypted password)           │                     │
	│                                     │                     │
	│ ┌─────────────────────────────┐    │                     │
	│ │ Tables:                     │    │                     │
	│ │ • AspNetUsers               │    │                     │
	│ │ • AspNetRoles               │    │                     │
	│ │ • AspNetUserRoles           │    │                     │
	│ │ • Tickets                │    │                     │
	│ │ • TicketTypes            │    │                     │
	│ │ • TicketTimelines        │    │                     │
	│ │ • Videos                    │    │                     │
	│ │ • Documents                 │    │                     │
	│ │ • Companies                 │    │                     │
	│ │ • Departments               │    │                     │
	│ │ • Attachments               │    │                     │
	│ │ • Chats                     │    │                     │
	│ │ • Reporters                 │    │                     │
	│ │ • Employees                 │    │                     │
	│ │ • TempEmails                │    │                     │
	│ │ • Recordings                │    │                     │
	│ │ • Relations                 │    │                     │
	│ │ + Identity Tables           │    │                     │
	│ └─────────────────────────────┘    │                     │
	└─────────────────────────────────────┘                     │
																 │
	External Services (Optional)                               │
	┌─────────────────────────────────────────────────────────┘
	│ • Email Service (SMTP Configuration)
	│ • Active Directory / LDAP (if configured)
	│ • File Scanning / Antivirus (if implemented)
	│ • External Logging Services
```

---

## 3. Network & Communication Details (Ports and Protocols)

### 3.1 Internal System Communication

| Component | Protocol | Port | Direction | Purpose | Notes |
|-----------|----------|------|-----------|---------|-------|
| **Client ↔ Web Server** | HTTP | 5118 | Request/Response | Local development access | Development only |
| **Client ↔ Web Server** | HTTPS | 7127 | Request/Response | Secure production access | Production/Testing |
| **Client ↔ Web Server (IIS)** | HTTPS | 44304 | Request/Response | IIS Express development | Development |
| **Client ↔ Web Server (IIS)** | HTTP | 42922 | Request/Response | IIS Express development | Development |
| **Web Server ↔ SQL Server** | TCP | 1433 | SQL Query/Response | Database communication | TrustServerCertificate=True |
| **Web Server ↔ SMTP Server** | SMTP/TLS | 587 (or 25/465) | Email Transmission | Outbound email notifications | Requires configuration |

### 3.2 HTTPS Configuration

**Production HTTPS Requirements:**
- The service **MUST operate over HTTPS (secure protocol)** for production deployments
- Current development URLs support HTTPS through IIS Express and local certificates
- In `launchSettings.json`:
  - HTTPS URL: `https://localhost:7127`
  - HTTP URL: `http://localhost:5118` (development only)
- Production deployment requires:
  - Valid SSL/TLS certificate from trusted certificate authority
  - HSTS (HTTP Strict Transport Security) headers enabled
  - Redirect HTTP to HTTPS

### 3.3 Database Connection

**Connection String:** `Data Source=10.0.10.154;Initial Catalog=ShareIT_lastTest;Persist Security Info=True;User ID=sa;Password=***;Connection Timeout=1000;TrustServerCertificate=True;MultipleActiveResultSets=True`

- **Server**: 10.0.10.154 (Remote SQL Server)
- **Port**: 1433 (Default SQL Server port)
- **Database**: ShareIT_lastTest
- **Authentication**: SQL Server Authentication (sa account)
- **Connection Timeout**: 1000ms
- **Certificate Trust**: Enabled (for development)
- **MARS**: Enabled (Multiple Active Result Sets)

---

## 4. Technologies and Frameworks Utilized

### 4.1 Frontend Technologies
| Technology | Version | Purpose |
|------------|---------|---------|
| **HTML5** | 5.0 | Page markup and structure |
| **CSS3** | 3.0 | Styling and responsive design |
| **Bootstrap** | 5.x | CSS framework for responsive UI components |
| **JavaScript** | ES6+ | Client-side interactivity and validation |
| **jQuery** | 3.x | DOM manipulation and AJAX |
| **jQuery Validation** | 1.x | Client-side form validation |
| **Razor Pages** | ASP.NET Core 8 | Server-side templating and page models |

### 4.2 Backend Framework & Runtime
| Component | Version | Purpose |
|-----------|---------|---------|
| **.NET** | 8.0 | Runtime environment and framework |
| **ASP.NET Core** | 8.0 | Web framework |
| **Entity Framework Core** | Latest | ORM for database access |
| **ASP.NET Core Identity** | 8.0 | Authentication, authorization, user management |

### 4.3 Database
| Technology | Version | Purpose |
|------------|---------|---------|
| **SQL Server** | 2019+ | Relational database management system |
| **Entity Framework Core** | Latest | ORM and migration management |

### 4.4 Authentication & Authorization
| Component | Purpose |
|-----------|---------|
| **ASP.NET Core Identity** | User authentication and management |
| **Role-Based Access Control (RBAC)** | Permission-based access control |
| **Claims-Based Authorization** | Fine-grained permission control |
| **Password Policy** | Enforced complexity requirements |
| **Email Confirmation** | Verified email addresses for accounts |
| **Account Lockout** | 5 failed attempts, 5-minute lockout |

### 4.5 Key NuGet Packages
```
• Microsoft.AspNetCore.Identity.EntityFrameworkCore
• Microsoft.EntityFrameworkCore.SqlServer
• Microsoft.AspNetCore.Identity.UI
• System.Text.Json (with ReferenceHandler for cycles)
```

### 4.6 Project Structure
- **Architecture Pattern**: MVC (Model-View-Controller)
- **View Engine**: Razor Pages
- **Project Type**: ASP.NET Core Web Application
- **Target Framework**: .NET 8.0
- **SDK Style**: Modern SDK-style project format

---

## 5. External Access Requirements

### 5.1 Authentication Requirements
- **User Registration**: Required for system access
- **Email Confirmation**: Must confirm email before first login
- **Multi-Step Authentication**: Optional (can be configured)
- **Password Requirements**:
  - Minimum length: 6 characters
  - Must contain uppercase letter
  - Must contain lowercase letter
  - Must contain digit
  - No non-alphanumeric characters required

### 5.2 External Integrations
| Service | Requirement | Configuration |
|---------|-------------|----------------|
| **SMTP Email Server** | Required | Configured in `appsettings.json` |
| **SQL Server** | Required | Remote server at 10.0.10.154 |
| **Active Directory** | Optional | Can be integrated for enterprise auth |
| **File Storage** | Internal | Local file system (wwwroot/uploads/) |

### 5.3 Access Control Policies
- **Default Access**: Anonymous users can access public pages
- **Protected Resources**: Requires authentication via `[Authorize]` attribute
- **Role-Based Access**: Controllers and actions filtered by user roles
- **Permission-Based Access**: Granular permissions defined in system

### 5.4 Session Management
- **Session Timeout**: 30 minutes of inactivity
- **Session Cookie**: HttpOnly (prevents JavaScript access)
- **Cookie Security Policy**: Uses HTTPS when available
- **Cookie Name**: `.ShareIT.Session`

---

## 6. The URL of the Service - HTTPS Secure Protocol

### 6.1 Development Environment URLs

| Environment | Protocol | URL | Port | Status |
|-------------|----------|-----|------|--------|
| **Development (HTTP)** | HTTP | `http://localhost:5118` | 5118 | Testing only |
| **Development (HTTPS)** | HTTPS | `https://localhost:7127` | 7127 | Preferred development |
| **IIS Express (HTTP)** | HTTP | `http://localhost:42922` | 42922 | Local testing |
| **IIS Express (HTTPS)** | HTTPS | `https://localhost:44304` | 44304 | Secure local testing |

### 6.2 Production Environment Requirements

**HTTPS REQUIREMENT - MANDATORY**
```
URL Format: https://{domain}/{controller}/{action}/{id}

Example:
- https://shareit.company.com
- https://shareit.company.com/Ticket/New
- https://shareit.company.com/Videos/Index
- https://shareit.company.com/Documents/Index
```

### 6.3 SSL/TLS Certificate Configuration

**Required for Production:**
1. Obtain valid SSL/TLS certificate from trusted Certificate Authority
2. Install certificate on production server
3. Configure binding in IIS to use HTTPS with certificate
4. Enforce HTTP → HTTPS redirection via:
   - IIS URL Rewrite module, OR
   - ASP.NET middleware: `app.UseHttpsRedirection();`

**Current Configuration in Program.cs:**
```csharp
if (app.Environment.IsProduction())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts(); // HTTP Strict Transport Security
}
app.UseHttpsRedirection(); // Enforce HTTPS
```

### 6.4 Security Headers for HTTPS

Recommended headers:
```
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Content-Security-Policy: default-src 'self'
```

---

## 7. The Intended Method of Access

### 7.1 Primary Access Method
**Direct URL Access**
- Users access the application directly via web browser
- URL: `https://{production-domain}`
- No embedding in other applications (currently)

### 7.2 Access Methods by User Type

| User Type | Access Method | URL Pattern |
|-----------|---------------|------------|
| **Reporters (Employees)** | Direct URL | `https://domain/Ticket/New` |
| **Managers** | Direct URL | `https://domain/FollowTicket/Index` |
| **Administrators** | Direct URL | `https://domain/Home/Dashboard` |
| **Viewers** | Direct URL | `https://domain/Search/Index` |

### 7.3 Authentication Flow

```
1. User navigates to https://domain
   ↓
2. Check if authenticated
   ├─ YES → Redirect to Dashboard
   └─ NO → Redirect to Login
   ↓
3. User enters credentials
   ↓
4. System validates against SQL Server database
   ↓
5. Email confirmation check
   ├─ NOT CONFIRMED → Send confirmation email
   └─ CONFIRMED → Check role/permissions
   ↓
6. Create session cookie (HttpOnly, Secure)
   ↓
7. Redirect to appropriate dashboard based on role
   ↓
8. Access granted to permitted resources
```

### 7.4 Session Access Pattern

```
Browser → (HTTPS) → Web Server → Load User Context
						 ↓
					Check Session Cookie
						 ↓
					Validate Claims & Roles
						 ↓
					Execute Action (if authorized)
						 ↓
					Return Response (HTTPS)
```

---

## 8. HTTP Methods Required & File Upload Capabilities

### 8.1 HTTP Methods Used

| HTTP Method | Purpose | Controllers Using |
|------------|---------|-------------------|
| **GET** | Retrieve data/pages | All controllers (Index, Details, Create forms) |
| **POST** | Submit data/create resources | All controllers (Create, Edit, Delete) |
| **PUT** | Update entire resource | Video/Document editing |
| **DELETE** | Delete resources | Video, Document, User deletion |
| **PATCH** | Partial updates | Used in API-like operations |

### 8.2 File Upload Capabilities

#### 8.2.1 Video Uploads

**Controller**: `VideosController`

**Allowed Extensions**:
- `.mp4` - MPEG-4 Video
- `.avi` - Audio Video Interleave
- `.mov` - QuickTime Movie
- `.wmv` - Windows Media Video
- `.flv` - Flash Video
- `.mkv` - Matroska Video
- `.webm` - WebM Video

**File Size Limits**:
- **Maximum**: 500 MB
- **Validation**: Client-side and server-side

**Upload Endpoint**:
```
POST /Videos/Create
Content-Type: multipart/form-data

Parameters:
- Section: string (100 chars max)
- VideoFile: IFormFile
```

**Storage Location**:
```
Physical: {WebRoot}/uploads/videos/
URL: /uploads/videos/{guid}.{extension}
Example: /uploads/videos/2028f1a4-544d-4232-94cd-9af5b2e934a3.webm
```

**Video Metadata Captured**:
- FileName: Original uploaded filename
- FileType: Extension (mp4, avi, mov, etc.)
- FileSize: Bytes
- VideoPath: Stored path
- CreatedOn: Upload timestamp
- Section: Category/Section name
- Duration: (Optional - can be populated via FFmpeg)

#### 8.2.2 Document Uploads

**Controller**: `DocumentController`

**Allowed File Types**:
- **PDF Files**: `.pdf` (Primary document format)
- **Logo Files**: Image formats
  - `.png`
  - `.jpg` / `.jpeg`
  - `.gif`

**File Size Limits**:
- **Documents**: Limited by IIS/server configuration (typically 100-500 MB)
- **Logos**: Typically 10-50 MB for images

**Upload Endpoints**:
```
POST /Document/Create
Content-Type: multipart/form-data

Parameters:
- Name: string (200 chars max)
- Description: string (500 chars max)
- PdfFile: IFormFile (Required)
- LogoFile: IFormFile (Optional)
- DisplayOrder: int
```

**Storage Locations**:
```
PDF Documents:
Physical: {WebRoot}/uploads/documents/
URL: /uploads/documents/{guid}.pdf

Logos:
Physical: {WebRoot}/uploads/logos/
URL: /uploads/logos/{guid}.{extension}
```

#### 8.2.3 Ticket Attachments

**Controller**: `TicketController`

**Supported Formats**:
- Document files
- Image files
- Media files

**File Size Limits**:
- **Maximum Per File**: 10 MB (defined as `MAX_FILE_SIZE = 10485760`)
- **Multiple Attachments**: Supported

**Storage Location**:
```
Physical: {WebRoot}/CompFiles/
URL: /CompFiles/{ticket-id}/{filename}
Example: /CompFiles/19042603203977/Admin/Screenshot.png
```

#### 8.2.4 Chat File Uploads

**Storage Location**:
```
Physical: {WebRoot}/uploads/chat/
Supported: PDF, Images, Documents
Examples:
- /uploads/chat/098e0d23-9919-4978-9958-d77fea6acda3.pdf
- /uploads/chat/e4191d3c-c518-475f-b8da-b0b4048c6742.png
```

### 8.3 API Endpoints Reference

#### 8.3.1 Ticket Management
```
GET    /Ticket/New                    - Display ticket form
POST   /Ticket/New                    - Submit new ticket
POST   /Ticket/CreateFromEmail        - Create ticket from email
GET    /FollowTicket/Index            - View ticket status
GET    /FollowTicket/AllTickets    - View all tickets
```

#### 8.3.2 Video Management
```
GET    /Videos/Index                     - List all videos
GET    /Videos/Create                    - Display upload form
POST   /Videos/Create                    - Upload video
GET    /Videos/Details/{id}              - View video details
GET    /Videos/Edit/{id}                 - Display edit form
POST   /Videos/Edit/{id}                 - Update video
GET    /Videos/Delete/{id}               - Display delete confirmation
POST   /Videos/Delete/{id}               - Delete video
```

#### 8.3.3 Document Management
```
GET    /Document/Index                   - List all documents
GET    /Document/Create                  - Display upload form
POST   /Document/Create                  - Upload document
GET    /Document/Edit/{id}               - Display edit form
POST   /Document/Edit/{id}               - Update document
GET    /Document/Delete/{id}             - Delete document
```

#### 8.3.4 User & Access Management
```
GET    /Users/Index                      - List users
POST   /Users/ManageRoles                - Assign roles to users
POST   /Users/AssignCompany              - Assign company to user
GET    /Roles/Index                      - List roles
POST   /Roles/ManagePermissions          - Configure role permissions
```

#### 8.3.5 System Management
```
GET    /Companies/Index                  - List companies
POST   /Companies/Create                 - Create company
GET    /Departments/Index                - List departments
POST   /Departments/Create               - Create department
GET    /TicketTypes/Index             - List ticket types
POST   /TicketTypes/Create            - Create ticket type
```

#### 8.3.6 Search & Discovery
```
GET    /Search/Index                     - Advanced search interface
POST   /Search/Query                     - Execute search query
```

### 8.4 File Upload Security

**Validations Implemented**:
1. **File Extension Validation**: Whitelist of allowed extensions
2. **File Size Validation**: Maximum file size enforcement
3. **MIME Type Validation**: Content-Type verification
4. **Anti-CSRF Protection**: ValidateAntiForgeryToken on all POST requests
5. **Virus/Malware Scanning**: Can be integrated (currently optional)

**Best Practices**:
- Files stored outside web root when possible
- Unique filenames (GUID-based) prevent traversal attacks
- Proper access control on file download endpoints
- Regular backup of uploaded files

### 8.5 Form Validation

**Client-Side**:
- jQuery Validation plugin
- Bootstrap form validation classes
- Real-time error display

**Server-Side**:
- Model State validation
- Data annotations on models
- Custom business logic validation
- Database constraint enforcement

---

## 9. Core Domain Models & Data Schema

### 9.1 Key Entity Relationships

```
Users (AspNetCore Identity)
├── Tickets (One User → Many Tickets)
├── Reporters (One User → Many Reporters)
├── Employees (One User → Many Employees)
└── UserRoles (Many-to-Many with Roles)

Tickets
├── TicketTypes (One Type → Many Tickets)
├── Companies (One Company → Many Tickets)
├── Departments (One Department → Many Tickets)
├── Reporters (One Reporter → Many Tickets)
├── TicketTimelines (One Ticket → Many Timeline Events)
├── Attachments (One Ticket → Many Attachments)
├── Chats (One Ticket → Many Chat Messages)
└── Records (One Ticket → Many Recordings)

TicketTypes
└── SubTicketTypes (One Type → Many Subtypes)

Companies
└── Departments (One Company → Many Departments)

Videos
└── (Standalone metadata with file storage)

Documents
└── (Standalone with logo relationship)

Attachments
├── Ticket (Many-to-One)
└── Chat (Many-to-One)

Chats
└── Ticket (Many-to-One)

TicketTimeline
└── Ticket (Many-to-One)
```

### 9.2 Security & Permission Model

```
Roles (Defined in database)
├── Admin
├── Manager
├── Reviewer
└── Reporter

Permissions (Defined per Role)
├── Create Ticket
├── View Ticket
├── Edit Ticket
├── Delete Ticket
├── Manage Users
├── Manage Roles
├── View Reports
├── Upload Documents
└── Upload Videos

Claims-Based Authorization
├── Permission: string
├── Module: string
├── Resource: string
└── Action: string
```

---

## 10. Deployment Considerations

### 10.1 Production Deployment Checklist

- [ ] SSL/TLS certificate installed and valid
- [ ] HTTPS enforced (HTTP redirects to HTTPS)
- [ ] HSTS headers configured
- [ ] Database connection string secured (encrypted in config)
- [ ] Email service SMTP configured
- [ ] Password policy enforced
- [ ] Logging and monitoring configured
- [ ] Regular backups scheduled
- [ ] File upload storage location secured
- [ ] Antivirus scanning integrated
- [ ] Rate limiting implemented
- [ ] DDoS protection configured
- [ ] Web Application Firewall (WAF) enabled
- [ ] Security headers configured

### 10.2 Performance Optimization

- Entity Framework Query optimization (select only needed columns)
- Database indexing on frequently searched fields
- Caching for static content (videos, documents)
- Session state optimization
- File compression for uploads
- CDN for static assets consideration

### 10.3 Maintenance & Monitoring

- Daily backup verification
- Database maintenance tasks
- Log file rotation and archiving
- Performance metrics monitoring
- Security patch management
- User activity auditing

---

## 11. Development Guidelines

### 11.1 Code Organization

```
/Areas/Identity         - Authentication & Authorization pages
/Controllers            - MVC Controllers (API logic)
/Views                  - Razor Pages (UI templates)
/Models                 - Entity models and ViewModels
/Data                   - DbContext and migrations
/Service                - Business logic services
/wwwroot/uploads        - File storage
```

### 11.2 Adding New Features

1. Create model in `/Models`
2. Add DbSet to `ApplicationDbContext`
3. Create migration: `dotnet ef migrations add MigrationName`
4. Create controller in `/Controllers`
5. Create views in `/Views/{Controller}`
6. Add routes in `Program.cs` if custom routing needed
7. Add permissions if role-based access required
8. Test all endpoints with proper authentication

### 11.3 Authentication Flow Summary

```
Anonymous User
	↓
Request protected resource
	↓
Redirect to /Identity/Account/Login
	↓
Enter credentials
	↓
Server validates (Users table + AspNetUsers)
	↓
Password verification
	↓
Email confirmation check
	↓
Load user claims & roles from AspNetUserRoles
	↓
Create authentication cookie (HttpOnly, Secure, 30min timeout)
	↓
Redirect to requested resource
	↓
User authenticated session established
```

---

## 12. Support & Troubleshooting

### Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| **401 Unauthorized** | User not authenticated or session expired | Re-login required |
| **403 Forbidden** | User lacks required role/permission | Contact administrator |
| **File upload fails** | Size/type validation | Check file requirements |
| **Database connection fails** | Network/credentials issue | Verify connection string |
| **Email not sending** | SMTP misconfiguration | Check email service config |
| **Slow performance** | Database queries | Optimize LINQ queries |

### Logging & Diagnostics

**Log Levels** (appsettings.json):
- `Default: Information` - General application events
- `Microsoft.AspNetCore: Warning` - Framework warnings only

**Enable Detailed Logging**:
```json
"Logging": {
  "LogLevel": {
	"Default": "Debug",
	"Microsoft.EntityFrameworkCore": "Information"
  }
}
```

---

## Document Summary

This documentation provides comprehensive coverage of the ShareIT application architecture, including:

✓ Application purpose and key functions  
✓ Detailed network architecture diagram  
✓ All ports and protocols used internally  
✓ Complete technology stack (Frontend, Backend, Database)  
✓ External access requirements and authentication  
✓ HTTPS configuration for secure operations  
✓ Primary access method (direct URL)  
✓ HTTP methods and file upload capabilities with limits  
✓ File extensions allowed and size constraints  
✓ Core data models and security architecture  
✓ Deployment and maintenance guidelines  

**Last Updated**: 2026  
**Application Version**: 1.0 (Based on .NET 8)  
**Database**: SQL Server (Remote at 10.0.10.154)
