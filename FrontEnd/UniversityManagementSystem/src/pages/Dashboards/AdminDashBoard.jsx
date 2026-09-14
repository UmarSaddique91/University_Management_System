import React, { useEffectEvent } from 'react'
import Layout from '../../layout/Layout'
import StatCard from '../../components/StatCard';
import EnrollmentTrend from '../../components/EnrollmentTrend';
import QuickActions from '../../components/QuickActions';
import { UserRound,Landmark,LibraryBig,NotebookTabs,UsersRound} from 'lucide-react'
import {useState, useEffect} from 'react';
import axios from 'axios'

const DashBoard = () => {


  // object for date on website
const currentDate= new Date().toLocaleDateString("en-GB",{

  weekday:"long",
  day:"2-digit",
  month:"long",
  year:"numeric"
});

//Use  state for cards data upgradation

const [dashboardData,SetDashboardData]= useState({
  totalStudents:0,
  totalTeachers:0,
  totalCourses:0,
  totalDepartments:0,
  activeEnrollments:0,
  studentThisMonth:0,
  studentGrowthPercentage:0,
  teacherThisMonth:0,
  teacherGrowthPercentage:0,
  enrollmentThisMonth:0,
  enrollmentGrowthPercentage:0
  
})

// UseEffect for cards data upgradition

useEffect(()=>{

  const fetchDashboardData=async()=>{
    try {

      const response= await axios.get(
        "https://localhost:7272/api/DashboardSummary/summary"
        
      )

      

      SetDashboardData(response.data)
      
    } catch (error) {
      console.log("Data noe fetched Successfully",error)
      
    }
  }

  fetchDashboardData();

},[])


  return (

    <Layout>
      <div>

        {/* DashboardIntro  */}
        <div className='flex justify-between items-center text-black'>
          <div>
            <h1 className='text-[18px] font-bold'>Dashboard</h1>
          <span className='text-[14px] text-slate-600'>Welcome back, Admin! Here's what's hapenning at your university. </span>
          </div>
          
          <div className='text-[14px] font-semibold'>
            {currentDate}
          </div>
          
          
        </div>


        {/* StatBoxes  */}
        
        <div className='flex justify-around grid grid-cols-5 gap-4 mt-3'>
          <StatCard

        Icon= {UserRound}
        Title="Total Studetns"
        Value={dashboardData.totalStudents}
        Change={`↑ ${dashboardData.studentGrowthPercentage}%`}
        SubText={`+${dashboardData.studentThisMonth} this month`}
        iconBg=" bg-blue-500/30"
        cardBg="bg-blue-500/5"
        iconcolor="text-blue-700"
        />

        <StatCard

        Icon= {UsersRound}
        Title="Total Teachers"
        Value={dashboardData.totalTeachers}
        Change={`↑ ${dashboardData.teacherGrowthPercentage}%`}
        SubText={`+${dashboardData.teacherThisMonth} this month`}
        iconBg=" bg-green-700/30"
        cardBg="bg-green-700/5"
        iconcolor="text-green-700"
        />

        <StatCard

        Icon= {NotebookTabs}
        Title="Total Courses"
        Value={dashboardData.totalCourses}
        
        iconBg=" bg-purple-700/30"
        cardBg="bg-purple-700/5"
        iconcolor="text-purple-700"
        />

        <StatCard

        Icon= {Landmark}
        Title="Total Departments"
        Value={dashboardData.totalDepartments}
        
        iconBg=" bg-yellow-700/30"
        cardBg="bg-yellow-700/5"
        iconcolor="text-yellow-700"
        
        />

        <StatCard

        Icon= {LibraryBig}
        Title="Active Enrollments"
        Value={dashboardData.activeEnrollments}
        Change={`↑ ${dashboardData.enrollmentGrowthPercentage}%`}
        SubText={`+${dashboardData.enrollmentThisMonth} this month`}
        iconBg=" bg-red-700/30"
        cardBg="bg-red-700/5"
        iconcolor="text-red-700"
        />
      </div>

        {/* Student Enrollment Trend & Quick Actions Section */}
        <div className='mt-4 flex flex-col xl:flex-row gap-4'>
          {/* Trend Chart (Maintains original full 62% width) */}
          <div className='w-full xl:w-[62%]'>
            <EnrollmentTrend />
          </div>

          {/* Quick Actions (Adjusted in the remaining 38% space next to Trend Chart) */}
          <div className='w-full xl:w-[38%]'>
            <QuickActions />
          </div>
        </div>
        </div>
    </Layout>
 
  )
}

export default DashBoard