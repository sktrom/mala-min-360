# QA Checklist

Use this checklist before a release candidate or deployment handoff.

## Backend QA

- Run `dotnet build apps/api/MalaMin.Api/MalaMin.Api.csproj`.
- Run `docker compose ps` and confirm PostgreSQL is healthy.
- Run EF migrations:

```powershell
dotnet ef database update --project apps/api/MalaMin.Api/MalaMin.Api.csproj --startup-project apps/api/MalaMin.Api/MalaMin.Api.csproj
```

- Start the API:

```powershell
dotnet run --project apps/api/MalaMin.Api/MalaMin.Api.csproj
```

- Verify:
  - `GET /`
  - `GET /api/health`
  - `GET /api/health/database`
  - `GET /api/health/model`

## Auth QA

- Login with `owner@demo.local` / `Demo12345!`.
- Confirm `POST /api/auth/login` returns an access token.
- Confirm `GET /api/auth/me` returns the current user.
- Confirm invalid credentials return `401`.
- Confirm protected endpoints reject missing tokens.

## Office User QA

- Login from `/login`.
- Confirm redirect to `/dashboard`.
- Confirm `/dashboard` loads stats.
- Confirm `/properties` loads real properties.
- Create a property.
- Publish and unpublish a property.
- Soft delete a property.
- Confirm plan limit errors show a readable Arabic message.
- Confirm `/subscription` loads the current plan and usage.

## Media And Images QA

- Upload a normal image.
- Confirm a MediaFile row is created.
- Link the image to a property.
- Set it as cover.
- Delete the image link.
- Confirm the public property page renders cover/gallery images.
- Confirm uploaded media URLs include CORS headers for the frontend origin in Development.

## Tour Rooms QA

- Upload a Panorama360 image.
- Create a tour room.
- Create a second room.
- Set a start room.
- Delete a room.
- Refresh and confirm remaining rooms persist.

## Hotspots QA

- Create a Navigate hotspot with a valid target room.
- Create an Info hotspot with `targetRoomId = null`.
- Edit hotspot label, yaw, and pitch.
- Confirm yaw validation between `-180` and `180`.
- Confirm pitch validation between `-90` and `90`.
- Delete a hotspot.
- Refresh and confirm remaining hotspots persist.

## Visitor QA

- Open `/` and confirm the entry gateway appears.
- Open `/visitor` without login.
- Confirm published properties appear.
- Confirm unpublished properties are not publicly visible.
- Open `/a/{tenantSlug}/{propertySlug}` without login.
- Confirm title, price, specs, gallery, contact actions, and 360 CTA render.
- Open `/a/{tenantSlug}/{propertySlug}/tour` without login.

## Public 360 Viewer QA

- Confirm Photo Sphere Viewer loads the panorama.
- Confirm mouse drag works.
- Confirm touch drag works on mobile.
- Confirm zoom works.
- Confirm fullscreen works where supported.
- Confirm room selector changes panorama.
- Confirm Navigate hotspots switch rooms.
- Confirm Info hotspots show information.
- Confirm no failed `/uploads` fetches and no CORS errors.

## Responsive QA

Check these viewport sizes:

- `390x844`
- `414x896`
- `768x1024`
- `1024x768`
- `1366x768`
- `1440x900`
- `1536x864`

Pages:

- `/`
- `/visitor`
- `/login`
- `/dashboard`
- `/properties`
- `/subscription`
- `/a/{tenantSlug}/{propertySlug}`
- `/a/{tenantSlug}/{propertySlug}/tour`

Expected:

- No horizontal scrolling on mobile.
- Buttons are touch-friendly.
- Forms are usable.
- Sidebar or navigation does not squeeze content.
- Cards stack cleanly.
- Public pages remain visually polished.

## Frontend QA

```powershell
cd apps/web
npm install
npm run build
```

Expected:

- Build succeeds.
- No TypeScript errors.
- No missing dependency errors.
- No Next.js route errors.

## Source Control QA

- Run `git status`.
- Confirm only intentional source and documentation changes are present.
- Confirm no `.env`, `.env.local`, uploaded files, `node_modules`, `bin`, or `obj` files are staged.
- Commit only reviewed changes.
