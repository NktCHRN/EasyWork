# ✨ What is it?
<p align="center">
  <img width="400" src="https://github.com/NktCHRN/EasyWork/blob/master/Demos/logo.png" alt="easywork's logo"/>
</p>
EasyWork is a simple kanban-style project management system. With it you can easily give tasks to your team, control how effectively and correctly they are performed and plan further actions. It impoves your teamwork and simplifies control and planning. It is written as a Web API and a website, so, you can design your own front-end if you want. Some of key features of this project will be listed below and we also recommend you to watch an overview in order to see these and other features in action via this link: https://www.youtube.com/watch?v=p6fdBQEY0Bw

# 🤖 Key features

## User-friendly interfaces
<p align="center">
  <img width="700" src="https://github.com/NktCHRN/EasyWork/blob/master/Demos/user-friendly.png" alt="a kanban board interface"/>
</p>
All interfaces on this project are user-friendly, so, it will not take much time to learn how to use them. Also, all pages are responsive too.

## Effective teamwork
<p align="center">
  <img width="700" src="https://github.com/NktCHRN/EasyWork/blob/master/Demos/teamwork.gif" alt="gif with adding the task to the board similtaniously on two browsers"/>
</p>
A kanban board, tasks, users on project, a project info, an archive and an admin-panel are dynamic. So, when one user of the project makes any changes, other users that are on the same page will see them too. Moreover, there are such useful features as currently active project participants (two circles at the top of the picture above) and user status (online/offline).

## Statistic tools
<p align="center">
  <img width="700" src="https://github.com/NktCHRN/EasyWork/blob/master/Demos/stats.png" alt="a Gantt chart"/>
</p>
Gantt chart allow use to control how fast tasks are done in your team in order to plan the next sprint better.

## Roles on project
<p align="center">
  <img width="700" src="https://github.com/NktCHRN/EasyWork/blob/master/Demos/roles-on-project.png" alt="sample project participants"/>
</p>
EasyWork boasts its system of roles on project. Currenty, there are three roles: Owner, Manager and User:

- User can participate in project: view information, other participants, an invite-code; add, edit and delete tasks;
- Manager has all user's abilities and also can update an invite-code and its' status, add users, kick participants with role "User" and change tasks' limits;
- Owner has full access to the project, including abilities to change the project information, to delete the project, to add, edit or kick any user (even another Owner). Be careful with granting this role!

## Admin-panel
<p align="center">
  <img width="700" src="https://github.com/NktCHRN/EasyWork/blob/master/Demos/admin.png" alt="an admin panel"/>
</p>
Sometimes, you have to block access for a user and this admin panel makes it much simpler. By the way, it is dynamic, so you can work together with other administrators.

# 🧑‍💻 Used technologies
On the back-end:

.NET 6

- Data: MS SQL Server, Entity Framework Core;
- Business: Automapper, MailKit, MimeKit;
- WebAPI: ASP.NET Core Web API, SignalR Core, Automapper, ASP.NET Core Identity, Swagger, HTMLAgilityPack, Google.Apis.Auth;
- Tests: NUnit, Moq, Microsoft.EntityFrameworkCore.InMemory;

and others.

On the front-end:
Angular v13, Angular Material v13, rxjs, auth0/angular-jwt, FontAwesome, Google Fonts, ngx-linky and others.

# 🗒 Notes
<p align="center">
  <img width="700" src="https://github.com/NktCHRN/EasyWork/blob/master/Demos/settings.png" alt="settings to change"/>
</p>
To run this project on your machine, you need to change JwtBearer, Mail and GoogleAuth settings, that are underlined in the picture above.

# 📝 License
This project is [MIT](https://github.com/NktCHRN/EasyWork/blob/master/LICENSE) licensed.
