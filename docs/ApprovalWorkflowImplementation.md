# Approval Workflow Module Implementation

## ✅ **Requirements Completed**

### **✅ User-Based Auto-Approval Logic**
- **Individual users** → Content is automatically approved upon submission
- **Team users** → Content requires manager approval before publishing  
- **Logic**: Based on `RoleType.Individual` vs team roles (`ContentCreator`, `SocialMediaManager`, etc.)

### **✅ API Endpoints Implemented**

#### **POST /api/content/submit**
```json
// Request
{
  "contentId": "guid"
}

// Response
{
  "success": true,
  "data": "content-guid",
  "message": "Content submitted for approval."
}
```

#### **POST /api/content/approve**  
```json
// Request
{
  "contentId": "guid",
  "comments": "Optional approval comments"
}

// Response  
{
  "success": true,
  "data": "content-guid", 
  "message": "Content approved successfully."
}
```

#### **GET /api/content/pending**
```json
// Request: ?teamId=guid

// Response
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "title": "Content Title",
      "body": "Content body text",
      "caption": "Optional caption",
      "hashtags": ["#tag1", "#tag2"],
      "submittedOnUtc": "2025-10-28T...",
      "author": {
        "id": "guid",
        "name": "John Doe",
        "email": "john@example.com"
      }
    }
  ]
}
```

### **✅ Database Status Management**
- **ContentStatus enum**: `Draft`, `Generated`, `PendingApproval`, `Approved`, `Rejected`, `Scheduled`, `Published`
- **ApprovalRecord entity**: Stores approval history with approver, status, comments, timestamps
- **ContentItem.CurrentApprovalId**: Links to active approval record

### **✅ Comprehensive Audit Logging**
All workflow actions are logged to `AuditLogs` table:

#### **Audit Actions Tracked:**
- `SUBMIT_FOR_APPROVAL` - When content is submitted for review
- `AUTO_APPROVE` - When individual user content is auto-approved  
- `APPROVE` - When manager approves content
- `REJECT` - When manager rejects content

#### **Audit Metadata (JSON):**
```json
{
  "contentTitle": "Post Title",
  "teamId": "guid",
  "approverId": "guid", 
  "comments": "Approval feedback",
  "reason": "Auto-approval for individual user"
}
```

## **🏗️ Architecture Implementation**

### **Command Handlers Enhanced**
- **`SubmitContentForApprovalCommand`**: Auto-approval logic + audit logging
- **`ApproveContentCommand`**: Manager approval + notifications + audit
- **`RejectContentCommand`**: Rejection handling + audit logging

### **Query Handler Added**  
- **`GetPendingContentQuery`**: Retrieves content awaiting approval for a team

### **Notification System Integration**
- **ApprovalRequested**: Notifies managers when content needs review
- **ApprovalDecision**: Notifies authors of approval/rejection decisions
- **Auto-approval**: Confirms individual user content was processed

### **Domain Events**
- **`ContentApprovalEvent`**: Fired on approve/reject actions
- **Integrated with existing event handling pipeline**

## **🔧 Implementation Details**

### **Role-Based Logic**
```csharp
// Auto-approve for Individual users
if (content.Author?.Role == RoleType.Individual)
{
    // Create auto-approval record and approve immediately
    var autoApproval = new ApprovalRecord(/*...*/);
    content.Approve(autoApproval.Id);
}
else  
{
    // Team users need manager approval
    content.SubmitForApproval(); 
    // Notify managers: Admin, SocialMediaManager, Approver roles
}
```

### **Manager Notification**
```csharp
var teamManagers = await _dbContext.Users
    .Where(u => u.TeamId == content.TeamId && 
               (u.Role == RoleType.Admin || 
                u.Role == RoleType.SocialMediaManager || 
                u.Role == RoleType.Approver))
    .ToListAsync();
```

## **🧪 Testing Coverage**

### **Integration Tests Created**
- ✅ Individual user auto-approval flow
- ✅ Team user requiring approval flow  
- ✅ Manager approval process
- ✅ Pending content retrieval
- ✅ Error handling for nonexistent content

### **Existing Tests**
- ✅ All existing ContentController tests continue to pass
- ✅ Content generation functionality unaffected

## **📋 Usage Examples**

### **Individual User Workflow**
```bash
# User creates content → submits → auto-approved
POST /api/content/submit { "contentId": "123" }
# → Status: Approved (immediate)
```

### **Team User Workflow**  
```bash
# Creator submits content
POST /api/content/submit { "contentId": "456" }  
# → Status: PendingApproval

# Manager reviews pending items
GET /api/content/pending?teamId=789
# → Returns list of pending content

# Manager approves
POST /api/content/approve { "contentId": "456", "comments": "LGTM!" }
# → Status: Approved
```

## **🔄 Integration with Existing System**

- **✅ Compatible** with existing content generation workflow
- **✅ Uses existing** notification and audit infrastructure  
- **✅ Extends existing** ContentController without breaking changes
- **✅ Preserves existing** database schema and relationships
- **✅ Maintains existing** authorization and authentication patterns

## **🚀 Ready for Production**

The approval workflow module is fully implemented and tested, providing:
- **Automated approval** for individual users  
- **Manager-gated approval** for team content
- **Complete audit trail** of all approval actions
- **RESTful API endpoints** for frontend integration
- **Comprehensive error handling** and validation