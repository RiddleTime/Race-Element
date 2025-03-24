import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';


@Component({
  standalone: true,
  template: `
  <div class="container mx-auto text-center select-none">
    <h2>.</h2>
  </div>
  `,
  imports: []
})
export default class AnalyticsComponent implements OnInit {
  constructor(private route: ActivatedRoute) {}

}