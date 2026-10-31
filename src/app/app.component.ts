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
        <a href="/" class="font-['Conthrax'] text-4xl md:text-6xl text-[orangered] italic uppercase tracking-wide">
          Race Element
        </a>
        <p class="text-center font-['Conthrax']">Solutions for Simulators</p>
        <p class="text-xs uppercase tracking-wider text-white/60 mt-1">21+ Sims · 40+ HUDs · Open Source</p>
      </div>

      <div class="mx-auto container place-content-center grid max-w-2xl md:max-w-3xl grid-cols-2 md:grid-cols-1">
        <nav class="font-['Conthrax'] text-sm md:text-base uppercase tracking-wide flex flex-wrap justify-center gap-4 md:gap-6 mt-3">
          <a routerLink="/news" routerLinkActive="text-[orangered] border-b-2 border-[orangered]" class="text-white/80 hover:text-[orangered] pb-1">News</a>
          <a routerLink="/guide" routerLinkActive="text-[orangered] border-b-2 border-[orangered]" [routerLinkActiveOptions]="{exact:true}" class="text-white/80 hover:text-[orangered] pb-1">Guides</a>
          <a routerLink="/guide/features" routerLinkActive="text-[orangered] border-b-2 border-[orangered]" class="text-white/80 hover:text-[orangered] pb-1">Features</a>
          <a routerLink="/guide/how-to-get-started" routerLinkActive="text-[orangered] border-b-2 border-[orangered]" class="text-white/80 hover:text-[orangered] pb-1">Download</a>
        </nav>

        <app-socials class="mr-6 md:mx-auto" />
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
