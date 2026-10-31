import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { SocialsComponent } from "./components/socials/socials.component";
import { RouteMeta } from '@analogjs/router';


export const routeMeta: RouteMeta = {
  meta: [
    {
      property: 'og:title',
      content: 'Race Element',
    },
    {
      property: 'og:description',
      content: 'Solutions for Sim Racing',
    },
    {
      property: 'twitter:title',
      content: 'Race Element',
    },
  ],
};

@Component({
    selector: 'app-root',
    template: `
    <div class="h-1 w-full bg-[orangered]"></div>
    <div class="container mx-auto flex flex-col self-center pt-2 select-none">

      <div class="mx-auto mb-3">
        <a href="/" class="hud-title font-['Conthrax'] text-3xl sm:text-4xl md:text-6xl">
          Race Element
        </a>
        <p class="text-center font-['Conthrax']">Solutions for Simulators</p>
        <p class="text-xs uppercase tracking-wider text-white/60 mt-1 text-center">21+ Sims · 40+ HUDs · Open Source</p>
      </div>

      <div class="flex flex-col items-center gap-2 mt-3">
        <nav class="font-['Conthrax'] text-xs sm:text-sm md:text-base uppercase tracking-wide flex flex-wrap justify-center gap-x-2 sm:gap-x-3 md:gap-x-4 gap-y-1">
          <a routerLink="/news" routerLinkActive="text-[orangered] border-b-2 border-[orangered]" class="text-white/80 hover:text-[orangered] pb-1">News</a>
          <a routerLink="/guide" routerLinkActive="text-[orangered] border-b-2 border-[orangered]" [routerLinkActiveOptions]="{exact:true}" class="text-white/80 hover:text-[orangered] pb-1">Guides</a>
          <a routerLink="/guide/features" routerLinkActive="text-[orangered] border-b-2 border-[orangered]" class="text-white/80 hover:text-[orangered] pb-1">Features</a>
          <a routerLink="/guide/how-to-get-started" routerLinkActive="text-[orangered] border-b-2 border-[orangered]" class="text-white/80 hover:text-[orangered] pb-1">Download</a>
        </nav>
        <app-socials />
      </div>

      <router-outlet class="mt-4 md:mt-4"></router-outlet>

      <footer class="text-sm select-none mx-auto mt-2">
        <img alt="GitHub Downloads (specific asset, all releases)" src="https://img.shields.io/github/downloads/riddletime/race-element/RaceElement.exe?style=flat&label=Downloads&color=%23FF4500">
        <p class="text-center">© 2022 - {{ThisYear}} Reinier Klarenberg</p>
      </footer>
    </div>
  `,
    styles: [
        `
    `,
    ],
    imports: [RouterOutlet, RouterLink, RouterLinkActive, SocialsComponent]
})
export class AppComponent {
  ThisYear: number = new Date().getFullYear();
}
