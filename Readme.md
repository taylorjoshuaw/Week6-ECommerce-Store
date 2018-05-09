# Strickland Propane

### We are deployed on Azure!

[https://stricklandpropane.azurewebsites.net](https://stricklandpropane.azurewebsites.net)

## Description

Strickland Propane is a mock e-commerce site built with ASP.NET Core 2.0,
Entity Framework, Identity Framework, Microsoft Azure, OAuth, SendGrid,
and Authorize.Net.

## Product

Strickland Propane sells propane and propane accessories. We deliver all of
your propane needs straight to your door (if you are in Texas) so you can
taste the meat, not the heat.

## Claims

Claims regarding the users' home state and grilling preference are collected
at the time of registration. These are based on the user's answers collected
on the registration form and are stored in the ApplicationUser entity. These
claims are needed to support the TexansOnly policy and the
PropaneAdvocatesOnly policy described below under the policies section.

## Policies

Policies used by the Strickland Propane website to authorize users' access
to certain portions of the site are as follows:

### AdminOnly

AdminOnly is a role-based policy used to only allow administrative users
access to the Product controller to perform CRUD operations on the products
available for purchase on the store. In addition, users who are authorized
under the AdminOnly policy can add existing users to the Admin role (needed to
be authorized for the AdminOnly policy) in addition to updating and deleting
existing users.

### MemberOnly

MemberOnly is a role-based policy met by any users with the Member role, which
is granted to all users upon registering for the website. The MemberOnly policy
is used to authorize users to create shopping baskets, add items to their
shopping baskets, update quantities of items in shopping baskets, delete items
from their shopping baskets, make purchases, and to view / update their
personal profile on the site. This role is granted to all new users regardless
of whether they manually register their account or if they use OAuth to login
using an external provider such as Twitter or Google.

### TexansOnly

TexansOnly is a claims-based policy met by any users claiming to be from Texas.
This policy is used to control to whom products can be delivered, since the
costs of shipping quality propane and propane accessories using traditional
shipping providers would be prohibitive. Non-Texans can still make purchases
through the Strickland Propane e-commerce platform but will need to make their
own arrangements to pick up their purchases from one of Strickland Propane's
friendly locations.

### PropaneAdvocatesOnly

PropaneAdvocatesOnly is a claims-based policy met by any users claiming to
prefer grilling with propane over any other alternative heat source. This policy
is used to control which items appear in the shop based on the policy
requirement set by the administrators when creating new items for the shop.
The primary example of this would be the propane tank exchange which only makes
sense for the user if they are already using propane to taste the meat, not the
heat.

## OAuth

Strickland Propane uses OAuth to allow users to authenticate themselves using
external OAuth providers. Currently, external OAuth logins are supported
using Google and Twitter. External logins due not require the user to create
a password or click on an e-mail verification link as the burden of
authentication is placed on the OAuth providers which have authenticated the
user on our behalf.

## E-mail

Sending e-mails is provided via the SendGrid cloud-based e-mail service
using the official SendGrid NuGet package. E-mails are used to authenticate
users against their e-mail addresses using verification codes generated via
the Identify platform's GenerateEmailConfirmationTokenAsync method. A
verification code is sent to users' e-mail addresses when registering a local
account without using an external OAuth provider. Once the user clicks on
the link provided in that e-mail, the EmailConfirmed property is set to true
for that user in the database allowing that user to log in to the website.

## Code Coverage

Currently, Strickland Propane has 0% code coverage. This will be addressed
as soon as possible using the xUnit testing framework and Moq for mocking
state for the purposes of testing.

## Azure Deployment

[https://stricklandpropane.azurewebsites.net](https://stricklandpropane.azurewebsites.net)

## Change Log

- 5.8.2018 - [Joshua Taylor](mailto:taylor.joshua88@gmail.com) - Initial
release. No unit testing. 0% code coverage.