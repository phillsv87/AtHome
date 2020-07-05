import React, { useMemo } from 'react';
import { Animated, View } from 'react-native';
import Page from './Page';
import { ViewRoute, ViewMatch, Navigation } from '../CommonJs/Navigation-rn';
import NavigationAnimation, { NavigationAnimationTypes, shouldUseNativeDriver } from '../CommonJs/NavigationAnimation-rn';
import { useApp } from '../lib/hooks';
import StreamsLayout from './StreamsLayout';
import HsApp from '../lib/HsApp';
import Location from './Location';
import Locations from './Locations';
import { unused } from '../CommonJs/commonUtils';
import LocationConfig from './LocationConfig';

function toIds(id:string|null):number[]
{
    if(!id){
        return [];
    }
    return id.split(',').map(i=>Number(i));
}

/* eslint react/display-name:0 */
function getRoutes(app:HsApp):ViewRoute[]{
    unused(app);
    return [
        {
            path:'/',
            postRender: page,
            render:()=><Locations/>
        },
        {
            path:'/add-location',
            postRender: page,
            render:()=><LocationConfig add/>
        },
        {
            match:/^\/location\/([^/]+)\/stream\/([\d,]+)$/i,
            postRender: pageSlim,
            render:(m)=><StreamsLayout locationId={m.param(0)||undefined} streamIds={toIds(m.param(1))}/>
        },
        {
            match:/^\/location\/([^/]+)\/config$/i,
            postRender: page,
            render:(m)=><LocationConfig locationId={m.param(0)||undefined}/>
        },
        {
            match:/^\/location\/([^/]+)$/i,
            postRender: page,
            render:(m)=><Location locationId={m.param(0)||undefined}/>
        },
    ];
}

const animationType:NavigationAnimationTypes='slide';
const nativeDriver=shouldUseNativeDriver(animationType,animationType);

function page(view: any, match: ViewMatch, direction: 'in' | 'out', animation: Animated.Value) {
    return (
        <NavigationAnimation type={animationType} animation={animation} direction={direction}>
            <Page>{view}</Page>
        </NavigationAnimation>
    )
}

function pageSlim(view: any, match: ViewMatch, direction: 'in' | 'out', animation: Animated.Value) {
    return (
        <NavigationAnimation type={animationType} animation={animation} direction={direction}>
            <View style={{flex:1,overflow:'hidden'}}>
                {view}
            </View>
        </NavigationAnimation>
    )
}

export function Router()
{
    const app=useApp();
    const routes=useMemo(()=>getRoutes(app),[app]);

    return (
        <Navigation nativeDriver={nativeDriver} routes={routes} />
    );
}